namespace AeroChef.AI.Agents.Agents.Interfaces
{
    public interface IAeroAgent
    {
        string AgentName { get; }
        Task<string> ExecuteAsync(string userQuery);
    }
}
