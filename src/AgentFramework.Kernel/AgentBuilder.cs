namespace AgentFramework.Kernel;

public sealed class AgentBuilder
{
    private readonly List<IAgentMiddleware> _mws = new();
    private readonly List<ITool> _tools = new();
    private IPolicy? _policy;
    private IMemoryStore? _memory;
    private readonly Dictionary<string, object> _meta = new();

    public AgentBuilder UsePolicy(IPolicy policy) { _policy = policy; return this; }
    public AgentBuilder UseMemory(IMemoryStore memory) { _memory = memory; return this; }
    public AgentBuilder UseMiddleware(IAgentMiddleware mw) { _mws.Add(mw); return this; }
    public AgentBuilder AddTool(ITool tool) { _tools.Add(tool); return this; }
    public AgentBuilder Set<T>(string key, T value) { _meta[key] = value!; return this; }

    public (Agent Agent, AgentContext Context) Build(AgentId id)
    {
        var toolReg = new InMemoryToolRegistry(_tools);
        var mem = _memory ?? new Agent.NullMemoryStore(); // internal type; or duplicate a NullMemoryStore here
        var agent = new Agent(_policy ?? new EchoPolicy(), _mws, mem);
        var ctx = new AgentContext(id, new List<Message>(), toolReg, mem, new DictionaryMetadata(_meta));
        return (agent, ctx);
    }

    private sealed class InMemoryToolRegistry : IToolRegistry
    {
        private readonly Dictionary<string, ITool> _dict;
        public InMemoryToolRegistry(IEnumerable<ITool> tools)
            => _dict = tools.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
        public ITool? Get(string name) => _dict.TryGetValue(name, out var t) ? t : null;
        public IEnumerable<ITool> All() => _dict.Values;
    }

    private sealed class DictionaryMetadata : IMetadataBag
    {
        private readonly Dictionary<string, object> _values;
        public DictionaryMetadata(Dictionary<string, object> values) => _values = values;
        public T? Get<T>(string key) => _values.TryGetValue(key, out var v) ? (T)v : default;
        public void Set<T>(string key, T value) => _values[key] = value!;
    }

    private sealed class EchoPolicy : IPolicy
    {
        public Task<Decision> DecideAsync(AgentContext context, CancellationToken ct = default)
        {
            var user = context.Conversation.LastOrDefault(m => m.Role == "user");
            var reply = new Message("assistant", user?.Content ?? string.Empty);
            return Task.FromResult(new Decision(reply, Array.Empty<ToolCall>()));
        }
    }
}
