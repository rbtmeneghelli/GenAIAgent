using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GenAiAgent.AI.Models;
using GenAiAgent.AI.Providers.Abstractions;
using GenAiAgent.Core;
using GenAiAgent.Core.Agents.Abstractions;
using GenAiAgent.Core.Enums;
using GenAiAgent.Core.Models;
using OpenAI;
using OpenAI.Chat;

namespace GenAiAgent.AI.Agents;

public class TitleGeneratorAgent(
    ILogger<TitleGeneratorAgent> logger,
    [FromKeyedServices(PromptProvider.File)]
    IPromptProvider promptProvider
) : IAgent<IEnumerable<Article>, string>
{
    private const string Name = "TitleGeneratorAgent";
    private const string Prompt = "Gere um título para a newsletter semanal com base neste JSON: ";
    private const float Temperature = 0.7f;

    public async Task<string> RunAsync(IEnumerable<Article> data, CancellationToken cancellationToken)
    {
        logger.LogInformation("• Gerando o título da newsletter...");

        var client = new OpenAIClient(Configuration.OpenAi.ApiKey);
        var instructions = await promptProvider.GetPromptAsync(Name, cancellationToken);

        var agent = client
            .GetChatClient(AiModels.Gpt4OMini)
            .AsAIAgent(new ChatClientAgentOptions
            {
                Name = Name,
                Description = "Agente especialista em gerar título para newsletter",
                ChatOptions = new ChatOptions
                {
                    ModelId = AiModels.Gpt4OMini,
                    Temperature = Temperature,
                    Instructions = instructions
                }
            });

        var prompt = $"{Prompt} {JsonSerializer.Serialize(data)}";
        var response = await agent.RunAsync<string>(prompt, cancellationToken: cancellationToken);

        logger.LogInformation("• Título gerado: {Title}", response.Result);

        return response.Result;
    }
}