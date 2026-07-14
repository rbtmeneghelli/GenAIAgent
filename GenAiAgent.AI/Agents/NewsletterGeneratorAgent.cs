using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GenAiAgent.AI.Models;
using GenAiAgent.AI.Providers.Abstractions;
using GenAiAgent.Core.Agents.Abstractions;
using GenAiAgent.Core.Enums;
using GenAiAgent.Core.Models;

namespace GenAiAgent.AI.Agents;

public class NewsletterGeneratorAgent(
    ILogger<NewsletterGeneratorAgent> logger,
    [FromKeyedServices(PromptProvider.File)]IPromptProvider promptProvider) : IAgent<IEnumerable<Article>, string>
{
    private const string AgentName = "NewsletterGeneratorAgent";
    private const string Prompt = "Gere um conteúdo para newsletter com base neste JSON: ";
    private const float Temperature = 0.7f;
    
    public async Task<string> RunAsync(
        IEnumerable<Article> data, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("• Gerando o conteúdo da newsletter...");

        var client = new OpenAIClient(Configuration.OpenAi.ApiKey);
        var instructions = await promptProvider.GetPromptAsync(AgentName, cancellationToken);

        var agent = client
            .GetChatClient(AiModels.Gpt4OMini)
            .AsAIAgent(new ChatClientAgentOptions
            {
                Name = AgentName,
                Description = "Agente especialista em gerar conteúdo para newsletter via E-mail",
                ChatOptions = new ChatOptions
                {
                    ModelId = AiModels.Gpt4OMini,
                    Temperature = Temperature,
                    Instructions = instructions
                }
            });

        var prompt = $"{Prompt} {JsonSerializer.Serialize(data)}";
        var response = await agent.RunAsync<string>(prompt, cancellationToken: cancellationToken);
        
        logger.LogInformation("• Newsletter gerada...");
        logger.LogInformation("---");
        logger.LogInformation(response.Result);
        logger.LogInformation("---");
        
        return response.Result;
    }
}