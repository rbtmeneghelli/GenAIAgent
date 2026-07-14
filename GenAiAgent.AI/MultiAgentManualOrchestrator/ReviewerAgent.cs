namespace GenAiAgent.AI.MultiAgentManualOrchestrator;

public sealed class ReviewerAgent
{
    public async Task<string> ExecuteAsync(string code)
    {
        var agent = new OpenAIClient(Configuration.OpenAi.ApiKey)
        .GetChatClient("gpt-4o-mini")
        .AsAIAgent(new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                Instructions = $""" Você é um revisor de código. Analise o código abaixo e sugira melhorias: {code} """,
            },
            Name = "ReviewerAgent"
        });

        var session = await agent.CreateSessionAsync();
        var answer = await agent.RunAsync(session);
        return answer.Text;
    }
}

