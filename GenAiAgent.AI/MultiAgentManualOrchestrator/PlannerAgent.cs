namespace GenAiAgent.AI.MultiAgentManualOrchestrator;

public sealed class PlannerAgent
{
    public async Task<string> ExecuteAsync(string request)
    {
        var agent = new OpenAIClient(Configuration.OpenAi.ApiKey)
        .GetChatClient("gpt-4o-mini")
        .AsAIAgent(new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                Instructions = $""" Você é um arquiteto de software. Crie um plano de implementação para: {request} """,
            },
            Name = "PlannerAgent"
        });

        var session = await agent.CreateSessionAsync();
        var answer = await agent.RunAsync(session);
        return answer.Text;
    }
}
