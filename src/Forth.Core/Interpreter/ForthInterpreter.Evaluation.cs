using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forth.Core.Interpreter;

// Partial: core evaluation loop
public partial class ForthInterpreter
{
    /// <summary>
    /// Evaluates a single line of Forth source text, loading prelude first if not yet loaded.
    /// </summary>
    /// <param name="line">Source line to evaluate.</param>
    /// <returns>True if interpreter continues running; false if exit requested.</returns>
    public async Task<bool> EvalAsync(string line)
    {
        if (!_preludeLoaded) await _loadPrelude;
        return await EvalInternalAsync(line);
    }

    private async Task<bool> EvalInternalAsync(string line)
    {
        _currentSourceId = -1; // string evaluation
        ArgumentNullException.ThrowIfNull(line);

        // Track current source and reset input index for this evaluation
        // If a refill buffer exists (from REFILL), prefer it as the current source
        // only when EvalAsync was not provided with an explicit line to evaluate
        _currentSource = _refillSource ?? line;
        // Reset >IN to 0 for new evaluation (ANS Forth requirement)
        _mem[_inAddr] = 0;
        // Tokenize the line - .( ... ) constructs become tokens that are processed during evaluation
        _tokens = Tokenizer.Tokenize(line);
        // Track character positions of tokens for WORD/token synchronization
        _tokenCharPositions = ComputeTokenPositions(line, _tokens);

        // ANS Forth bracket conditional multi-line support:
        // If we're currently skipping due to a false [IF], scan this line for
        // [IF], [ELSE], [THEN] to track nesting and potentially resume execution
        if (_bracketIfSkipping)
        {
            return await ProcessSkippedLine();
        }

        // Preprocess idiomatic compound tokens like "['] name" or "[']name" into
        // the equivalent sequence: "[" "'" name "]" so existing primitives
        // ([, ', ]) handle the compile-time behaviour without adding new words.
        if (_tokens is not null && _tokens.Count > 0)
        {
            var processed = new List<string>();
            for (int ti = 0; ti < _tokens.Count; ti++)
            {
                var t = _tokens[ti];
                if (t == "[']")
                {
                    // If a following token exists, consume it as the target name.
                    if (ti + 1 < _tokens.Count)
                    {
                        processed.Add("[");
                        processed.Add("'");
                        processed.Add(_tokens[ti + 1]);
                        processed.Add("]");
                        ti++; // skip the consumed name
                        continue;
                    }
                    // Fallback: expand to [ and ' tokens and continue
                    processed.Add("[");
                    processed.Add("'");
                    continue;
                }

                // Handle generic bracketed composite like: [ IF ] -> "[IF]"
                if (t == "[" && ti + 2 < _tokens.Count && _tokens[ti + 2] == "]")
                {
                    var inner = _tokens[ti + 1];
                    // Only synthesize known bracketed forms to avoid changing other uses
                    var upper = inner.ToUpperInvariant();
                    if (upper == "IF" || upper == "ELSE" || upper == "THEN")
                    {
                        processed.Add("[" + upper + "]");
                        ti += 2; // consume inner and closing bracket
                        continue;
                    }
                }

                processed.Add(t);
            }
            _tokens = processed;
        }

        _tokenIndex = 0;

        while (TryReadNextToken(out var tok))
        {
            if (tok.Length == 0)
            {
                continue;
            }

            if (_doesCollecting)
            {
                // Stop DOES> collection when semicolon is encountered
                // The semicolon will be processed normally to finish the definition
                if (tok == ";")
                {
                    // Don't add semicolon to DOES> tokens
                    // Process it normally to finish definition (which will apply DOES> body)
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
                    
                    // Check if bracket conditional changed state to skipping
                    // If so, process remaining tokens in skip mode inline
                    if (_bracketIfSkipping)
                    {
                        // Skip tokens until we find matching [THEN] or [ELSE]
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
                                    // End of skipped section - resume normal processing
                                    _bracketIfSkipping = false;
                                    _bracketIfSeenElse = false;
                                    _bracketIfNestingDepth = 0;
                                    _bracketIfActiveDepth--;
                                    break;
                                }
                            }
                            else if (skipUpper == "[ELSE]" && _bracketIfNestingDepth == 0)
                            {
                                // At our depth - resume execution
                                _bracketIfSkipping = false;
                                _bracketIfSeenElse = true;
                                break;
                            }
                            // All other tokens are skipped
                        }
                    }
                    
                    continue;
                }

                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {tok}");
            }
            else
            {
                // Handle .( ... ) immediate printing even in compile mode
                if (tok.StartsWith(".(") && tok.EndsWith(")") && tok.Length >= 3)
                {
                    WriteText(tok.Substring(2, tok.Length - 3));
                    continue;
                }

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
                        
                        // Check if bracket conditional changed state to skipping
                        // If so, we need to process remaining tokens in skip mode inline
                        if (_bracketIfSkipping)
                        {
                            // Skip tokens until we find matching [THEN] or [ELSE]
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
                                        // End of skipped section - resume normal processing
                                        _bracketIfSkipping = false;
                                        _bracketIfSeenElse = false;
                                        _bracketIfNestingDepth = 0;
                                        _bracketIfActiveDepth--;
                                        break;
                                    }
                                }
                                else if (skipUpper == "[ELSE]" && _bracketIfNestingDepth == 0)
                                {
                                    // At our depth - resume execution
                                    _bracketIfSkipping = false;
                                    _bracketIfSeenElse = true;
                                    break;
                                }
                                // All other tokens are skipped
                            }
                        }
                        
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

                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word in definition: {tok}");
            }
        }

        // DOES> finalization now happens in FinishDefinition() when semicolon is encountered,
        // not at end of evaluation. This allows multiple CREATE-DOES> pairs in one file.

        _tokens = null; // clear stream
        return !_exitRequested;
    }
}
