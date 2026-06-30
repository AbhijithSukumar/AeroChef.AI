using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Client;

public class AgentFactory
{
    private readonly IServiceProvider _serviceProvider;
    public AgentFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    // Ensure this signature matches your call: (string, string, string)
    public async Task<ChatClientAgent> CreateAgentAsync(string role, string instructions, IEnumerable<AITool>? tools = null)
    {

        var chatClient = _serviceProvider.GetRequiredService<IChatClient>();
        // 1. Setup secure HttpClient with your headers
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-ms-api-role", role);

        // 2. Setup MCP transport
        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri("http://localhost:5000/mcp")
        }, httpClient);

        var mcpClient = await McpClient.CreateAsync(transport);
        var aiTools = (await mcpClient.ListToolsAsync()).Cast<AITool>().ToList();
        if (tools != null) aiTools.AddRange(tools);

        // 3. Return the ChatAgent
        return new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                Instructions = instructions,
                Tools = aiTools
            }
        });
    }
}
