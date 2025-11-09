namespace Forth.Core.Modules;

public interface IForthWordModule
{
    void Register(Forth.Core.IForthInterpreter forth);
}
