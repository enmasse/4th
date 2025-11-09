namespace Forth.ProtoActor;

/// <summary>
/// Request message sent to a ForthInterpreterActor to evaluate Forth source code.
/// </summary>
/// <param name="Source">Single line of Forth code to evaluate.</param>
public sealed record ForthEvalRequest(string Source);

/// <summary>
/// Response message from a ForthInterpreterActor containing continuation flag and stack snapshot.
/// </summary>
/// <param name="Continue">True if interpreter did not request exit.</param>
/// <param name="Stack">Interpreter parameter stack snapshot (top is last element).</param>
public sealed record ForthEvalResponse(bool Continue, object[] Stack);
