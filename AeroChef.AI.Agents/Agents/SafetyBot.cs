using AeroChef.AI.Agents.Agents.Interfaces;
using System.Diagnostics;

namespace AeroChef.AI.Agents.Agents
{
    public class SafetyBot : IAeroAgent
    {
        // Consistent source name allows for unified filtering in LangSmith
        private static readonly ActivitySource MyActivitySource = new("AeroChef.Agents");

        private readonly AgentFactory _factory;

        public SafetyBot(AgentFactory factory)
        {
            _factory = factory;
        }

        public string AgentName => "SafetyBot";

        public async Task<string> ExecuteAsync(string userQuery)
        {
            using (var activity = MyActivitySource.StartActivity("SafetyBot.Execute"))
            {
                // 1. Add GenAI Semantic Conventions
                activity?.SetTag("gen_ai.system", "gemini");
                activity?.SetTag("gen_ai.prompt.0.content", userQuery);
                activity?.SetTag("gen_ai.prompt.0.role", "user");

                // 1. Create the specialized agent instance. 
                // The factory automatically loads the HACCP and Temperature tools from DAB.
                var agent = await _factory.CreateAgentAsync(
                    "SafetyOfficer",
                    "You are a HACCP Compliance Officer. Use the provided tools to query temperature logs and audit compliance records. Report any deviations immediately."
                );

                // 2. Run the agent. 
                // The LLM will now use the MCP tools to interact with the database 
                // instead of returning a hardcoded string.
                var response = await agent.RunAsync(userQuery);

                // 2. Add Completion/Response metadata
                if (response.Text != null)
                {
                    activity?.SetTag("gen_ai.completion.0.content", response.Text);
                    activity?.SetTag("gen_ai.completion.0.role", "assistant");
                }

                if (response.Text?.Contains("deviation", StringComparison.OrdinalIgnoreCase) == true)
                {
                    activity?.AddEvent(new ActivityEvent("HACCP_DEVIATION_DETECTED"));
                }

                return response.Text ?? "Safety audit complete.";
            }
           
        }
    }
}
