using GenAiAgent.AI.MultiAgentManualOrchestrator;
using GenAiAgent.Core.Repositories.Abstractions;
using GenAiAgent.Core.Services.Abstractions;
using GenAiAgent.Infra.Factory;
using GenAiAgent.Infra.Repositories;
using GenAiAgent.Infra.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GenAiAgent.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<INewsletterService, NewsletterService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<DeveloperAgent>();
        services.AddScoped<PlannerAgent>();
        services.AddScoped<ReviewerAgent>();
        services.AddScoped<OrcherstratorAgent>();
        services.AddTransient<IGenAIFactory, GenAIFactory>();

        return services;
    } 
    
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<ISubscriberRepository, SubscriberRepository>();
        
        return services;
    }
}