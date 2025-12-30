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
        // Ensure prelude has been loaded before any evaluation.
        if (!_preludeLoaded)
        {
            var prevTrace = EnableTrace;
            EnableTrace = prevTrace || Environment.GetEnvironmentVariable("FORTH_TRACE_PRELUDE") == "1";
            Trace("EvalAsync: awaiting prelude load");
            await _loadPrelude.ConfigureAwait(false);
            Trace($"EvalAsync: prelude loaded flag={_preludeLoaded}");
            EnableTrace = prevTrace;
        }

        return await EvalInternalAsync(line).ConfigureAwait(false);
    }

    private async Task<bool> EvalInternalAsync(string line)
    {
        _currentSourceId = -1; // string evaluation
        ArgumentNullException.ThrowIfNull(line);

        // If REFILL staged a new input line, allow callers to advance the interpreter
        // by invoking EvalAsync("") (or whitespace) to consume that staged input.
        // This keeps parsing source aligned with SOURCE/REFILL in multi-call test scenarios.
        // NOTE: REFILL stages input for SOURCE; do not implicitly interpret _refillSource on empty EvalAsync calls.

        // Always parse from the parameter line
        // _refillSource is kept separate and only used by SOURCE primitive via CurrentSource property
        _currentSource = line;

        // Create fresh parser for the source
        _parser = new CharacterParser(_currentSource ?? string.Empty);

        // Reset >IN to 0 for new evaluation (ANS Forth requirement)
        _mem[_inAddr] = 0;
        _parser.SetPosition(0);

        string? lastTok = null;

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

        // NEW: Character-based parsing loop (replaces token-based loop)
        while (TryParseNextWord(out var tok))
        {
            if (tok.Length == 0)
            {
                continue;
            }

            lastTok = tok;

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
                Trace($"TOK {tok} (interp)");

                // Treat "..." as an interpret-time string literal.
                // This is a convenience used heavily by the test suite, and it also
                // aligns with INCLUDED, which accepts a string on the data stack.
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

                Trace($"UNDEF {tok} isCompiling={_isCompiling}");
                throw new ForthException(ForthErrorCode.UndefinedWord, $"Undefined word: {tok}");
            }
            else
            {
                Trace($"TOK {tok} (compile)");
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

        if (_isCompiling)
        {
            // Allow interactive/multi-call compilation where a definition spans multiple EvalAsync calls.
            // Only treat end-of-input as an error when there is no active definition context.
            if (_currentDefName is null && _compilationStack.Count == 0)
            {
                var src = _currentSource ?? string.Empty;
                var preview = src.Length > 200 ? src[..200] + "..." : src;
                var last = lastTok ?? "<none>";
                throw new ForthException(ForthErrorCode.CompileError,
                    $"Unexpected end-of-input while compiling (last token: '{last}'). Source preview: {preview}");
            }
        }

        return !_exitRequested;
    }
}
