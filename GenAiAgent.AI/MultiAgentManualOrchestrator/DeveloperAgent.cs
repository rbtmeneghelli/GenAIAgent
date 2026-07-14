namespace GenAiAgent.AI.MultiAgentManualOrchestrator;

public sealed class DeveloperAgent
{
    public async Task<string> ExecuteAsync(string plan)
    {
        var agent = new OpenAIClient(Configuration.OpenAi.ApiKey)
        .GetChatClient("gpt-4o-mini")
        .AsAIAgent(new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                Instructions = $""" Você é um desenvolvedor .NET. Gere o código para o seguinte plano: {plan} """,
            },
            Name = "DeveloperAgent"
        });

        var session = await agent.CreateSessionAsync();
        var answer = await agent.RunAsync(session);
        return answer.Text;
    }
}
