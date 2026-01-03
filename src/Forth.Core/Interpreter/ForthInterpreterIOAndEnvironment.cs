using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using Forth.Core.Binding;

namespace Forth.Core.Interpreter;

internal sealed class ForthInterpreterIOAndEnvironment
{
    private readonly ForthInterpreter _i;

    public ForthInterpreterIOAndEnvironment(ForthInterpreter i)
    {
        _i = i;
    }

    internal void WriteNumber(long n) => _i.IO.PrintNumber(n);

    internal void NewLine() => _i.IO.NewLine();

    internal void WriteText(string s) => _i.IO.Print(s);

    internal int ReadKey() => _i.IO.ReadKey();

    internal bool KeyAvailable() => _i.IO.KeyAvailable();

    internal string? ReadLineFromIO() => _i.IO.ReadLine();

    internal int LoadAssemblyWords(Assembly asm) =>
        AssemblyWordLoader.RegisterFromAssembly(_i, asm);

    internal void AddEnvWords()
    {
        _i._dict = _i._dict.SetItem(("ENV", "OS"), new Word(i => { i.Push(Environment.OSVersion.Platform.ToString()); return Task.CompletedTask; }) { Name = "OS", Module = "ENV" });
        _i._dict = _i._dict.SetItem(("ENV", "CPU"), new Word(i => { i.Push((long)Environment.ProcessorCount); return Task.CompletedTask; }) { Name = "CPU", Module = "ENV" });
        _i._dict = _i._dict.SetItem(("ENV", "MEMORY"), new Word(i => { i.Push(Environment.WorkingSet); return Task.CompletedTask; }) { Name = "MEMORY", Module = "ENV" });
        _i._dict = _i._dict.SetItem(("ENV", "MACHINE"), new Word(i => { i.Push(Environment.MachineName); return Task.CompletedTask; }) { Name = "MACHINE", Module = "ENV" });
        _i._dict = _i._dict.SetItem(("ENV", "USER"), new Word(i => { i.Push(Environment.UserName); return Task.CompletedTask; }) { Name = "USER", Module = "ENV" });
        _i._dict = _i._dict.SetItem(("ENV", "PWD"), new Word(i => { i.Push(Environment.CurrentDirectory); return Task.CompletedTask; }) { Name = "PWD", Module = "ENV" });
        _i._dict = _i._dict.SetItem(("ENV", "DATE"), new Word(i => { i.Push(DateTime.Now.ToString("yyyy-MM-dd")); return Task.CompletedTask; }) { Name = "DATE", Module = "ENV" });
        _i._dict = _i._dict.SetItem(("ENV", "TIME"), new Word(i => { i.Push(DateTime.Now.ToString("HH:mm:ss")); return Task.CompletedTask; }) { Name = "TIME", Module = "ENV" });

        var envVars = Environment.GetEnvironmentVariables();
        foreach (DictionaryEntry entry in envVars)
        {
            var name = (string)entry.Key;
            var value = entry.Value as string ?? "";
            _i._dict = _i._dict.SetItem(("ENV", name), new Word(i => { i.Push(value); return Task.CompletedTask; }) { Name = name, Module = "ENV" });
        }
    }
}
