using System.Threading.Tasks;
using Forth;
using Proto;

namespace Forth.ProtoActor;

/// <summary>
/// Proto.Actor actor that hosts a Forth interpreter and evaluates received code.
/// </summary>
public sealed class ForthInterpreterActor : IActor
{
    private readonly ForthInterpreter _forth = new();

    /// <inheritdoc />
    public Task ReceiveAsync(IContext context)
    {
        var msg = context.Message;
        switch (msg)
        {
            case ForthEvalRequest req:
                return HandleEval(context, req);
        }
        return Task.CompletedTask;
    }

    private async Task HandleEval(IContext ctx, ForthEvalRequest req)
    {
        bool cont;
        try
        {
            cont = await _forth.EvalAsync(req.Source);
        }
        catch (ForthException ex)
        {
            ctx.Respond(new ForthEvalResponse(false, new object[]{ ex.Code, ex.Message }));
            return;
        }
        ctx.Respond(new ForthEvalResponse(cont, _forth.Stack.ToArray()));
    }
}
