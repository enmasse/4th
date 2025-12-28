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

        // Always parse from the parameter line
        // _refillSource is kept separate and only used by SOURCE primitive via CurrentSource property
        _currentSource = line;
        
        // Create fresh parser for the source
        _parser = new CharacterParser(_currentSource);
        
        // Reset >IN to 0 for new evaluation (ANS Forth requirement)
        _mem[_inAddr] = 0;
        _parser.SetPosition(0);

        // ANS Forth bracket conditional multi-line support:
        // If we're currently skipping due to a false [IF], scan for bracket conditionals
        // to track nesting and potentially resume execution
        if (_bracketIfSkipping)
        {
            // Process skipped tokens until we exit skip mode or reach end of source
            await ProcessSkippedLine();
            
            // If still skipping (reached end while in skip mode), return early
            if (_bracketIfSkipping)
            {
                return !_exitRequested;
            }
            
            // Exited skip mode - fall through to process remaining tokens normally
        }

        // REMOVED: Token preprocessing no longer needed
        // CharacterParser handles bracket forms (['], [IF], [ELSE], [THEN]) directly during parsing

        // NEW: Character-based parsing loop (replaces token-based loop)
        while (TryParseNextWord(out var tok))
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

                // REMOVED: ABORT special handling - CharacterParser and ABORT"/ABORT primitives handle this
                // ABORT is a regular word that will be looked up and executed
                // ABORT" is handled by CharacterParser as a composite token and by the ABORT" primitive

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
                    
                    // Check if executing the word triggered skip mode (e.g., [ELSE] after TRUE [IF])
                    // If so, switch to ProcessSkippedLine to handle remaining tokens
                    if (_bracketIfSkipping)
                    {
                        // Enter skip mode - process remaining source in skip mode
                        await ProcessSkippedLine();
                        
                        // If still skipping after processing, done with this source
                        if (_bracketIfSkipping)
                        {
                            return !_exitRequested;
                        }
                        
                        // Otherwise, exited skip mode - continue with remaining tokens
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
                            // NEW: Use parse buffer for immediate word body expansion
                            _parseBuffer ??= new Queue<string>();
                            foreach (var token in cw.BodyTokens)
                            {
                                _parseBuffer.Enqueue(token);
                            }
                            continue;
                        }
                        
                        await cw.ExecuteAsync(this);
                        
                        // Check if executing immediate word triggered skip mode (e.g., [ELSE] after TRUE [IF])
                        // If so, switch to ProcessSkippedLine to handle remaining tokens
                        if (_bracketIfSkipping)
                        {
                            // Enter skip mode - process remaining source in skip mode
                            await ProcessSkippedLine();
                            
                            // If still skipping after processing, done with this source
                            if (_bracketIfSkipping)
                            {
                                return !_exitRequested;
                            }
                            
                            // Otherwise, exited skip mode - continue with remaining tokens
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

        _parseBuffer = null; // clear parse buffer (NEW - prevents token carryover between evaluations)
        // NOTE: Do NOT clear _refillSource here! It should persist until the next REFILL
        // This allows SOURCE to access the refilled content across multiple EvalAsync calls
        return !_exitRequested;
    }
}
