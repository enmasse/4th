using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

// Partial: bracket conditional skip mode processing
public partial class ForthInterpreter
{
    /// <summary>
    /// Process a line while in bracket conditional skip mode.
    /// Scans ONLY for [IF], [ELSE], [THEN] to track nesting depth and potentially resume execution.
    /// All other tokens are completely skipped.
    /// </summary>
    private async Task<bool> ProcessSkippedLine()
    {
        if (_tokens is null) return !_exitRequested;

        for (_tokenIndex = 0; _tokenIndex < _tokens.Count; _tokenIndex++)
        {
            var tok = _tokens[_tokenIndex];
            var upper = tok.ToUpperInvariant();

            // Handle composite bracket forms like "[" "IF" "]"
            if (tok == "[" && _tokenIndex + 2 < _tokens.Count && _tokens[_tokenIndex + 2] == "]")
            {
                var inner = _tokens[_tokenIndex + 1].ToUpperInvariant();
                if (inner == "IF" || inner == "ELSE" || inner == "THEN")
                {
                    upper = "[" + inner + "]";
                    _tokenIndex += 2; // consume the composite
                }
            }

            // Track [IF] nesting
            if (upper == "[IF]")
            {
                _bracketIfNestingDepth++;
                _bracketIfActiveDepth++;  // Track active depth even when skipping
                continue;
            }

            // Handle [THEN]
            if (upper == "[THEN]")
            {
                if (_bracketIfNestingDepth > 0)
                {
                    _bracketIfNestingDepth--;
                    _bracketIfActiveDepth--;  // Decrement active depth
                }
                else
                {
                    // End of our conditional block - resume execution
                    _bracketIfSkipping = false;
                    _bracketIfSeenElse = false;
                    _bracketIfNestingDepth = 0;
                    _bracketIfActiveDepth--;  // Decrement active depth
                    
                    // Process remaining tokens on this line
                    _tokenIndex++;
                    return await ContinueEvaluation();
                }
                continue;
            }

            // Handle [ELSE]
            if (upper == "[ELSE]")
            {
                if (_bracketIfNestingDepth == 0)
                {
                    // At our depth level - switch from skip to execute
                    _bracketIfSkipping = false;
                    _bracketIfSeenElse = true;
                    
                    // Process remaining tokens on this line
                    _tokenIndex++;
                    return await ContinueEvaluation();
                }
                // Nested [ELSE] - just skip it
                continue;
            }

            // All other tokens: just skip them (don't process)
            // This includes regular IF, THEN, ELSE, :, ;, and any other words
        }

        // Entire line skipped
        _tokens = null;
        return !_exitRequested;
    }

    /// <summary>
    /// Continue evaluation of remaining tokens after resuming from skip mode.
    /// </summary>
    private async Task<bool> ContinueEvaluation()
    {
        while (TryReadNextToken(out var tok))
        {
            if (tok.Length == 0)
            {
                continue;
            }

            // Handle bracket conditionals that might start skipping again
            var upper = tok.ToUpperInvariant();
            if (upper == "[IF]" || upper == "[ELSE]" || upper == "[THEN]")
            {
                // Execute these as immediate words
                if (TryResolveWord(tok, out var word) && word is not null)
                {
                    await word.ExecuteAsync(this);
                    
                    // If we transitioned to skip mode, switch to skip processing for rest of line
                    if (_bracketIfSkipping)
                    {
                        // Don't call ProcessSkippedLine - it would restart from tokenIndex=0
                        // Instead, manually skip remaining tokens on this line with proper nesting tracking
                        while (TryReadNextToken(out var skipTok))
                        {
                            var skipUpper = skipTok.ToUpperInvariant();
                            
                            // Handle composite bracket forms
                            if (skipTok == "[" && _tokenIndex + 1 < _tokens!.Count && _tokenIndex + 2 < _tokens.Count && _tokens[_tokenIndex + 1] == "]")
                            {
                                var inner = _tokens[_tokenIndex].ToUpperInvariant();
                                if (inner == "IF" || inner == "ELSE" || inner == "THEN")
                                {
                                    skipUpper = "[" + inner + "]";
                                    _tokenIndex += 2;
                                }
                            }
                            
                            // Track bracket conditional nesting
                            if (skipUpper == "[IF]")
                            {
                                _bracketIfNestingDepth++;
                                _bracketIfActiveDepth++;
                            }
                            else if (skipUpper == "[THEN]")
                            {
                                if (_bracketIfNestingDepth > 0)
                                {
                                    _bracketIfNestingDepth--;
                                    _bracketIfActiveDepth--;
                                }
                                else
                                {
                                    // Found matching [THEN] - end skip mode
                                    _bracketIfSkipping = false;
                                    _bracketIfSeenElse = false;
                                    _bracketIfNestingDepth = 0;
                                    _bracketIfActiveDepth--;
                                    break; // Resume normal processing
                                }
                            }
                            else if (skipUpper == "[ELSE]" && _bracketIfNestingDepth == 0)
                            {
                                // Found [ELSE] at our depth - resume execution
                                _bracketIfSkipping = false;
                                _bracketIfSeenElse = true;
                                break; // Resume normal processing
                            }
                            // All other tokens are skipped
                        }
                        
                        // If still skipping, we're done with this line
                        if (_bracketIfSkipping)
                        {
                            _tokens = null;
                            return !_exitRequested;
                        }
                        // Otherwise continue processing remaining tokens normally
                    }
                    continue;
                }
            }

            // Normal token processing (same as main eval loop)
            if (_doesCollecting)
            {
                if (tok == ";")
                {
                    FinishDefinition();
                    continue;
                }
                else
                {
                    _doesTokens?.Add(tok);
                    continue;
                }
            }

            if (!_isCompiling)
            {
                // Handle .( ... ) immediate printing
                if (tok.StartsWith(".(") && tok.EndsWith(")") && tok.Length >= 3)
                {
                    WriteText(tok.Substring(2, tok.Length - 3));
                    continue;
                }

                if (tok.Equals("ABORT", StringComparison.OrdinalIgnoreCase))
                {
                    if (TryReadNextToken(out var maybeMsg) && maybeMsg.Length >= 2 && maybeMsg[0] == '"' && maybeMsg[^1] == '"')
                    {
                        throw new ForthException(ForthErrorCode.Unknown, maybeMsg[1..^1]);
                    }
                    throw new ForthException(ForthErrorCode.Unknown, "ABORT");
                }

                if (tok.StartsWith("ABORT\"", StringComparison.OrdinalIgnoreCase))
                {
                    var msg = tok[6..^1];
                    throw new ForthException(ForthErrorCode.Unknown, msg);
                }

                // Automatic pushing of quoted string tokens
                // This allows standalone quoted strings like "path.4th" INCLUDED to work
                // Immediate parsing words like S" consume their tokens via ReadNextTokenOrThrow
                // before this code sees them, so there's no interference
                if (tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"')
                {
                    Push(tok[1..^1]);
                    continue;
                }

                if (TryParseNumber(tok, out var num))
                {
                    Push(num);
                    continue;
                }

                if (TryParseDouble(tok, out var dnum))
                {
                    Push(dnum);
                    continue;
                }

                if (TryResolveWord(tok, out var word) && word is not null)
                {
                    await word.ExecuteAsync(this);
                    continue;
                }

                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {tok}");
            }
            else
            {
                if (tok != ";")
                {
                    _currentDefTokens?.Add(tok);
                }

                if (tok == ";")
                {
                    FinishDefinition();
                    continue;
                }

                if (tok.Length >= 2 && tok[0] == '"' && tok[^1] == '"')
                {
                    CurrentList().Add(intr =>
                    {
                        intr.Push(tok[1..^1]);
                        return Task.CompletedTask;
                    });
                    continue;
                }

                if (TryParseNumber(tok, out var lit))
                {
                    CurrentList().Add(intr =>
                    {
                        intr.Push(lit);
                        return Task.CompletedTask;
                    });
                    continue;
                }

                if (TryParseDouble(tok, out var dlit))
                {
                    CurrentList().Add(intr =>
                    {
                        intr.Push(dlit);
                        return Task.CompletedTask;
                    });
                    continue;
                }

                if (TryResolveWord(tok, out var cw) && cw is not null)
                {
                    if (cw.IsImmediate)
                    {
                        if (cw.BodyTokens is { Count: > 0 })
                        {
                            _tokens!.InsertRange(_tokenIndex, cw.BodyTokens);
                            continue;
                        }

                        await cw.ExecuteAsync(this);
                        continue;
                    }

                    CurrentList().Add(async intr => await cw.ExecuteAsync(intr));
                    continue;
                }

                if (_currentLocals != null && _currentLocals.Contains(tok))
                {
                    CurrentList().Add(intr => { intr.Push(intr._locals![tok]); return Task.CompletedTask; });
                    continue;
                }

                // Handle .( ... ) immediate printing even in compile mode
                if (tok.StartsWith(".(") && tok.EndsWith(")") && tok.Length >= 3)
                {
                    WriteText(tok.Substring(2, tok.Length - 3));
                    continue;
                }

                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word in definition: {tok}");
            }
        }

        _tokens = null;
        return !_exitRequested;
    }
}
