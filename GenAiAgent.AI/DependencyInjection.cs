using Microsoft.Extensions.DependencyInjection;
using GenAiAgent.AI.Agents;
using GenAiAgent.AI.Providers;
using GenAiAgent.AI.Providers.Abstractions;
using GenAiAgent.Core.Agents.Abstractions;
using GenAiAgent.Core.Enums;
using GenAiAgent.Core.Models;

namespace GenAiAgent.AI;

public static class DependencyInjectionn
{
    public static IServiceCollection AddAgents(this IServiceCollection services)
    {
        services.AddKeyedTransient<IAgent<IEnumerable<Article>, string>, TitleGeneratorAgent>(AgentType.TitleGenerator);
        services.AddKeyedTransient<IAgent<IEnumerable<Article>, string>, NewsletterGeneratorAgent>(AgentType.NewsletterGenerator);
        
        services.AddKeyedTransient<IPromptProvider, FilePromptProvider>(PromptProvider.File);

        return services;
    }
}