namespace Forth.ProtoActor;

public sealed record ForthEvalRequest(string Source);
public sealed record ForthEvalResponse(bool Continue, object[] Stack);
