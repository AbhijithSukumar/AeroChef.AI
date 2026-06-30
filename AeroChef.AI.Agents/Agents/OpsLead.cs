using AeroChef.AI.Agents.Agents.Interfaces;
using Microsoft.Extensions.AI;;
using System.Diagnostics;

namespace AeroChef.AI.Agents.Agents
{
    public class OpsLead : IAeroAgent
    {
        // Reuse the same ActivitySource defined in your library
        private static readonly ActivitySource MyActivitySource = new("AeroChef.Agents");

        private readonly AgentFactory _factory;
        // 1. Add fields to hold the bots
        private readonly ChefBot _chefBot;
        private readonly SafetyBot _safetyBot;

        // 2. Update constructor to accept the bots
        public OpsLead(AgentFactory factory, ChefBot chefBot, SafetyBot safetyBot)
        {
            _factory = factory;
            _chefBot = chefBot;
            _safetyBot = safetyBot;
        }

        public string AgentName => "OpsLead";

        public async Task<string> ExecuteAsync(string userQuery)
        {
            using(var activity = MyActivitySource.StartActivity("OpsLead.Execute"))
            {
                // 1. Tag the initial user intent
                activity?.SetTag("gen_ai.system", "gemini");
                activity?.SetTag("gen_ai.prompt.0.content", userQuery);
                activity?.SetTag("gen_ai.prompt.0.role", "user");

                // 3. Define the tools locally (or as a private method)
                var tools = new List<AITool>
            {
                AIFunctionFactory.Create(
                    (string query) => _chefBot.ExecuteAsync(query),
                    "CallChefBot",
                    "Delegates food or inventory queries to the Chef."
                ),
                AIFunctionFactory.Create(
                    (string query) => _safetyBot.ExecuteAsync(query),
                    "CallSafetyBot",
                    "Delegates safety, HACCP, or hygiene queries to the Safety Officer."
                )
            };

                // 4. Pass tools to the factory (ensure your factory supports this signature)
                var agent = await _factory.CreateAgentAsync("OpsLead",
                    @"You are the Operations Lead. Your job is to delegate tasks. 
                  - If the query is about food, recipes, or inventory, call the CallChefBot tool.
                  - If the query is about safety, incidents, or hygiene, call the CallSafetyBot tool.",
                    tools);

                var response = await agent.RunAsync(userQuery);

                // 2. Tag the final coordination response
                if (response.Text != null)
                {
                    activity?.SetTag("gen_ai.completion.0.content", response.Text);
                    activity?.SetTag("gen_ai.completion.0.role", "assistant");
                }

                return response.Text ?? "OpsLead completed the coordination.";
            }
  
        }
    }
}
