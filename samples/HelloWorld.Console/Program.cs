using AgentFramework.Kernel;

public sealed class EchoPolicy : IPolicy
{
    public Task<Decision> DecideAsync(AgentContext ctx, CancellationToken ct = default)
    {
        var last = ctx.Conversation.LastOrDefault();
        if (last is { Role: "user" })
            return Task.FromResult(new Decision(new Message("assistant", $"Echo: {last.Content}"), Array.Empty<ToolCall>()));
        return Task.FromResult(new Decision(null, Array.Empty<ToolCall>()));
    }
}

class Program
{
    static async Task Main()
    {
        var (agent, ctx) = new AgentBuilder()
            .UsePolicy(new EchoPolicy())
            .Build(new AgentId("hello"));

        Console.WriteLine("Type anything (blank line to exit).");
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) break;

            ctx.Conversation.Add(new Message("user", input));
            await agent.StepAsync(ctx);

            var reply = ctx.Conversation.LastOrDefault(m => m.Role == "assistant");
            if (reply is not null)
                Console.WriteLine(reply.Content);
        }
    }
}
