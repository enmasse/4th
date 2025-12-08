using System.Collections.Immutable;
using System.Text;
using System.Globalization;
using System.IO;
using Forth.Core.Binding;

namespace Forth.Core.Interpreter;

// Partial: evaluation, parsing, and token handling
public partial class ForthInterpreter
{
    // Current source tracking (line and index within it)
    internal string? _currentSource;
    // Refill buffer set by REFILL primitive (takes precedence over current source)
    internal string? _refillSource;
    internal string? CurrentSource => _refillSource ?? _currentSource;

    internal long _currentSourceId;
    internal long SourceId => _currentSourceId;

    /// <summary>
    /// Sets the current source ID.
    /// </summary>
    /// <param name="id">The source ID.</param>
    public void SetSourceId(long id) => _currentSourceId = id;

    internal List<string>? _tokens; // internal current token stream
    internal int _tokenIndex;       // internal current token index
    internal List<int>? _tokenCharPositions; // character position of each token in source

    // Token reading helpers used by primitives and source-level operations
    internal bool TryReadNextToken(out string token)
    {
        // Check if >IN has been modified externally (e.g., by >IN ! to skip input)
        MemTryGet(_inAddr, out var inVal);
        var inPos = (int)ToLong(inVal);
        
        // If >IN points to or past the end of source, no more tokens
        if (_currentSource != null && inPos >= _currentSource.Length)
        {
            token = string.Empty;
            return false;
        }
        
        if (_tokens is null || _tokenIndex >= _tokens.Count)
        {
            token = string.Empty;
            return false;
        }
        
        // Skip tokens that start before the current >IN position
        // This handles both WORD-based character parsing and >IN ! manipulation
        while (_tokenCharPositions != null && _tokenIndex < _tokenCharPositions.Count)
        {
            var tokenStartPos = _tokenCharPositions[_tokenIndex];
            if (tokenStartPos >= inPos)
            {
                // This token starts at or after >IN, so it's valid
                break;
            }
            // This token was consumed or skipped, advance past it
            _tokenIndex++;
            if (_tokenIndex >= _tokens.Count)
            {
                token = string.Empty;
                return false;
            }
        }
        
        token = _tokens[_tokenIndex++];
        return true;
    }

    internal string ReadNextTokenOrThrow(string message)
    {
        if (!TryReadNextToken(out var t) || string.IsNullOrEmpty(t))
            throw new ForthException(ForthErrorCode.CompileError, message);
        return t;
    }

    /// <summary>
    /// Compute character positions of tokens in source line for synchronization with WORD primitive.
    /// </summary>
    private List<int>? ComputeTokenPositions(string source, List<string>? tokens)
    {
        if (tokens == null || tokens.Count == 0)
            return null;
        
        var positions = new List<int>();
        int searchStart = 0;
        
        foreach (var token in tokens)
        {
            // Find this token in the source starting from searchStart
            int pos = source.IndexOf(token, searchStart, StringComparison.Ordinal);
            if (pos >= 0)
            {
                positions.Add(pos);
                searchStart = pos + token.Length;
            }
            else
            {
                // Token not found (shouldn't happen), use approximate position
                positions.Add(searchStart);
            }
        }
        
        return positions;
    }

    // Public method for REFILL to set the current source
    /// <summary>
    /// Refills the current input source with the specified line, resetting the input index.
    /// </summary>
    /// <param name="line">The new input line.</param>
    public void RefillSource(string line)
    {
        // Store refill buffer separately so subsequent EvalAsync calls do not
        // overwrite the refill source when they set _currentSource for their
        // own command text. >IN is reset to 0 for the new refill buffer.
        _refillSource = line;
        _mem[_inAddr] = 0;
    }

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
        // reset memory >IN cell
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

    private bool TryParseNumber(string token, out long value)
    {
        long GetBase(long def)
        {
            MemTryGet(_baseAddr, out var b);
            return b <= 0 ? def : b;
        }

        return NumberParser.TryParse(token, GetBase, out value);
    }

    private bool TryParseDouble(string token, out double value)
    {
        value = 0.0;
        if (string.IsNullOrEmpty(token)) return false;
        
        // ANS Forth floating-point literal support:
        // 1. Decimal point: "1.5", "3.14"
        // 2. Scientific notation: "1.5e2", "3e-1", "1.5E+2"
        // 3. Forth shorthand: "1e" = 1.0, "2e" = 2.0, "1.0E" = 1.0 (e/E as type indicator)
        // 4. Optional trailing 'd'/'D' suffix: "1.5d"
        // 5. Special values: NaN, Infinity, -Infinity
        
        bool hasSuffixD = token.EndsWith('d') || token.EndsWith('D');
        string span = hasSuffixD ? token.Substring(0, token.Length - 1) : token;
        
        // Check for Forth shorthand notation: <number>e or <number>E (without exponent)
        // Examples: 1e = 1.0, 2e = 2.0, -3e = -3.0, 0e = 0.0, 1.0E = 1.0
        // This is when 'E' or 'e' is at the end with no digits after it
        if (span.Length >= 2 && (span.EndsWith('e') || span.EndsWith('E')))
        {
            var mantissa = span.Substring(0, span.Length - 1);
            // Try to parse the mantissa as an integer first (common case: "1e", "2e")
            if (long.TryParse(mantissa, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var intValue))
            {
                value = (double)intValue;
                return true;
            }
            // If that fails, try to parse as a floating-point number (e.g., "1.0E", "3.14E")
            if (double.TryParse(mantissa, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
        }
        
        // Check if this looks like a floating-point number
        bool looksFloating = span.Contains('.') 
            || span.Contains('e') || span.Contains('E')
            || span.IndexOf("NaN", StringComparison.OrdinalIgnoreCase) >= 0
            || span.IndexOf("Infinity", StringComparison.OrdinalIgnoreCase) >= 0;
        
        if (!looksFloating) return false;
        
        // Try to parse as double - ANS Forth allows simple decimal notation like "1.5"
        return double.TryParse(span, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
    }

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
