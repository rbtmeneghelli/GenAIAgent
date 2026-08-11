namespace GenAiAgent.AI.MultiAgentApproveCredit.Configuration;

public class AIConfiguration
{
    public const string SectionName = "OpenAI";
    public string Model { get; set; } = "gpt-4.1";
    public string ApiKey =>
        Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        ?? throw new InvalidOperationException(
            "A variável de ambiente OPENAI_API_KEY não foi encontrada.");
}