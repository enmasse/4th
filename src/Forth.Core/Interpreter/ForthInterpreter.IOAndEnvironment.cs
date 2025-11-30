using System;
using System.Collections;
using System.Reflection;
using Forth.Core.Binding;
using Forth.Core.Execution;

namespace Forth.Core.Interpreter;

// Partial: IO operations and environment words
public partial class ForthInterpreter
{
    /// <summary>
    /// Gets the IO implementation used by the interpreter.
    /// </summary>
    public IForthIO IO => _io;

    internal void WriteNumber(long n) =>
        _io.PrintNumber(n);

    internal void NewLine() =>
        _io.NewLine();

    internal void WriteText(string s) =>
        _io.Print(s);

    // Re-added IO/query methods used by primitives after refactor
    internal int ReadKey() => _io.ReadKey();
    internal bool KeyAvailable() => _io.KeyAvailable();
    internal string? ReadLineFromIO() => _io.ReadLine();

    /// <summary>
    /// Registers all words marked by <see cref="Forth.Core.Execution.PrimitiveAttribute"/> in a given assembly.
    /// </summary>
    /// <param name="asm">Assembly to scan.</param>
    /// <returns>Number of words registered.</returns>
    public int LoadAssemblyWords(Assembly asm) =>
        AssemblyWordLoader.RegisterFromAssembly(this, asm);

    private void AddEnvWords()
    {
        // OS name
        _dict = _dict.SetItem(("ENV", "OS"), new Word(i => { i.Push(Environment.OSVersion.Platform.ToString()); return Task.CompletedTask; }) { Name = "OS", Module = "ENV" });

        // CPU count
        _dict = _dict.SetItem(("ENV", "CPU"), new Word(i => { i.Push((long)Environment.ProcessorCount); return Task.CompletedTask; }) { Name = "CPU", Module = "ENV" });

        // Total memory (in bytes, as long)
        _dict = _dict.SetItem(("ENV", "MEMORY"), new Word(i => { i.Push(Environment.WorkingSet); return Task.CompletedTask; }) { Name = "MEMORY", Module = "ENV" });

        // Machine name
        _dict = _dict.SetItem(("ENV", "MACHINE"), new Word(i => { i.Push(Environment.MachineName); return Task.CompletedTask; }) { Name = "MACHINE", Module = "ENV" });

        // User name
        _dict = _dict.SetItem(("ENV", "USER"), new Word(i => { i.Push(Environment.UserName); return Task.CompletedTask; }) { Name = "USER", Module = "ENV" });

        // Current directory
        _dict = _dict.SetItem(("ENV", "PWD"), new Word(i => { i.Push(Environment.CurrentDirectory); return Task.CompletedTask; }) { Name = "PWD", Module = "ENV" });

        // Current date
        _dict = _dict.SetItem(("ENV", "DATE"), new Word(i => { i.Push(DateTime.Now.ToString("yyyy-MM-dd")); return Task.CompletedTask; }) { Name = "DATE", Module = "ENV" });

        // Current time
        _dict = _dict.SetItem(("ENV", "TIME"), new Word(i => { i.Push(DateTime.Now.ToString("HH:mm:ss")); return Task.CompletedTask; }) { Name = "TIME", Module = "ENV" });

        // Add all environment variables as words
        var envVars = Environment.GetEnvironmentVariables();
        foreach (DictionaryEntry entry in envVars)
        {
            var name = (string)entry.Key;
            var value = entry.Value as string ?? "";
            _dict = _dict.SetItem(("ENV", name), new Word(i => { i.Push(value); return Task.CompletedTask; }) { Name = name, Module = "ENV" });
        }
    }

    [Primitive("ENV", HelpString = "ENV ( -- wid ) - environment wordlist id")]
    private static Task Prim_ENV(ForthInterpreter i)
    {
        i.Push("ENV");
        return Task.CompletedTask;
    }
}