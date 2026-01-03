using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Forth.Core.Interpreter;

// Facade: preserve existing module management APIs on `ForthInterpreter`.
public partial class ForthInterpreter
{
    internal void WithModule(string name, Action action) => _moduleManagement.WithModule(name, action);

    internal ImmutableList<string?> GetOrder() => _moduleManagement.GetOrder();

    internal void SetOrder(IEnumerable<string?> order) => _moduleManagement.SetOrder(order);
}
