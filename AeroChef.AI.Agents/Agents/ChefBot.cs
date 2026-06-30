using AeroChef.AI.Agents.Agents.Interfaces;
using Microsoft.Agents.AI;
using System.Diagnostics;

namespace AeroChef.AI.Agents.Agents
{
    public class ChefBot : IAeroAgent
    {
        // Define a static ActivitySource for this class/library
        private static readonly ActivitySource MyActivitySource = new("AeroChef.Agents");

        private readonly AgentFactory _factory;

        public ChefBot(AgentFactory factory)
        {
            _factory = factory;
        }

        public string AgentName => "ChefBot";

        public async Task<string> ExecuteAsync(string userQuery)
        {
            using(var activity = MyActivitySource.StartActivity("ChefBot.Execute"))
            {
                // 1. Core Metadata
                activity?.SetTag("agent.name", AgentName);

                // 2. GenAI Semantic Conventions (Required for LangSmith audit trails)
                activity?.SetTag("gen_ai.system", "gemini");
                activity?.SetTag("gen_ai.prompt.0.content", userQuery);
                activity?.SetTag("gen_ai.prompt.0.role", "user");
                // 1. Create the specialized agent instance using the factory
                // This agent is pre-configured with the correct Role and MCP Tools
                var agent = await _factory.CreateAgentAsync("Chef", "You are a professional chef. Use tools to manage kitchen inventory and recipes.");

                // 2. Run the agent with the user's query
                // RunAsync handles the entire LLM loop, including tool calling
                var response = await agent.RunAsync(userQuery);

                // 3. Capture completion
                if (response.Text != null)
                {
                    activity?.SetTag("gen_ai.completion.0.content", response.Text);
                    activity?.SetTag("gen_ai.completion.0.role", "assistant");
                }

                // Add result details to the trace
                activity?.SetTag("agent.response_length", response.Text?.Length ?? 0);

                // 3. Extract and return the final text response
                return response.Text ?? "ChefBot completed the task.";
            } 
        }
    }
}
