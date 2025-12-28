using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

// Partial: bracket conditional skip mode processing
public partial class ForthInterpreter
{
    /// <summary>
    /// Process tokens while in bracket conditional skip mode.
    /// Scans ONLY for [IF], [ELSE], [THEN] to track nesting depth and potentially resume execution.
    /// All other tokens are completely skipped.
    /// Returns false if exit was requested, true otherwise.
    /// After calling this, check _bracketIfSkipping to see if still in skip mode.
    /// </summary>
    private Task<bool> ProcessSkippedLine()
    {
        // Skip mode - scan for bracket conditionals only
        while (_bracketIfSkipping && TryParseNextWord(out var tok))
        {
            if (tok.Length == 0) continue;
            
            var upper = tok.ToUpperInvariant();

            // Track [IF] nesting
            if (upper == "[IF]")
            {
                _bracketIfNestingDepth++;
                _bracketIfActiveDepth++;
                continue;
            }

            // Handle [THEN]
            if (upper == "[THEN]")
            {
                if (_bracketIfNestingDepth > 0)
                {
                    _bracketIfNestingDepth--;
                    _bracketIfActiveDepth--;
                }
                else
                {
                    // End of our conditional block - resume execution
                    _bracketIfSkipping = false;
                    _bracketIfSeenElse = false;
                    _bracketIfNestingDepth = 0;
                    _bracketIfActiveDepth--;
                    
                    // Exit skip mode - caller will continue with main loop
                    break;
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
                    
                    // Exit skip mode - caller will continue with main loop
                    break;
                }
                // Nested [ELSE] - just skip it
                continue;
            }

            // All other tokens: just skip them
        }

        return Task.FromResult(!_exitRequested);
    }

    /// <summary>
    /// Continue evaluation of remaining tokens after resuming from skip mode.
    /// </summary>
    private async Task<bool> ContinueEvaluation()
    {
        // UPDATED: Use character-based parsing instead of token-based
        while (TryParseNextWord(out var tok))
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
                    
                    // If we transitioned to skip mode, skip rest of line
                    if (_bracketIfSkipping)
                    {
                        // Skip remaining tokens on this line with proper nesting tracking
                        while (TryParseNextWord(out var skipTok))
                        {
                            var skipUpper = skipTok.ToUpperInvariant();
                            
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

                // REMOVED: ABORT special handling - CharacterParser and primitives handle this

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
                            // NEW: Use parse buffer for immediate word body expansion
                            _parseBuffer ??= new Queue<string>();
                            foreach (var token in cw.BodyTokens)
                            {
                                _parseBuffer.Enqueue(token);
                            }
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

        return !_exitRequested;
    }
}
