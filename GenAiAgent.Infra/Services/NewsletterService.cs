using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GenAiAgent.Core.Agents.Abstractions;
using GenAiAgent.Core.Enums;
using GenAiAgent.Core.Models;
using GenAiAgent.Core.Repositories.Abstractions;
using GenAiAgent.Core.Services.Abstractions;

namespace GenAiAgent.Infra.Services;

public class NewsletterService(
    ILogger<NewsletterService> logger,
    IArticleRepository articleRepository,
    
    [FromKeyedServices(AgentType.TitleGenerator)]
    IAgent<IEnumerable<Article>, string> titleGeneratorAgent,
    
    [FromKeyedServices(AgentType.NewsletterGenerator)]
    IAgent<IEnumerable<Article>, string> newsletterGeneratorAgent,
    
    ISubscriberRepository subscriberRepository,
    IEmailService emailService) : INewsletterService
{
    public async Task SendAsync(CancellationToken cancellationToken)
    {
        // Recupera os posts da semana
        logger.LogInformation("• Recuperando os posts da semana...");
        var posts = await articleRepository.GetFromLastWeekAsync(cancellationToken);
        if (!posts.Any())
            return;
        
        // Gera um título para a newsletter
        logger.LogInformation("• Gerando o título da newsletter...");
        var subject = await titleGeneratorAgent.RunAsync(posts, cancellationToken);
        
        // Gera o conteúdo da newsletter
        logger.LogInformation("• Gerando o conteúdo da newsletter...");
        var body = await newsletterGeneratorAgent.RunAsync(posts, cancellationToken);
        
        // Recupera os inscritos
        logger.LogInformation("• Recuperando os inscritos...");
        var subscribers = await subscriberRepository.GetAllAsync(cancellationToken);
        
        // Envia o E-mail
        logger.LogInformation("• Enviando a newsletter para os inscritos...");
        foreach (var subscriber in subscribers)
            await emailService.SendAsync(subscriber.Name, subscriber.Email, subject, body, cancellationToken);

        logger.LogInformation("• Finalizado...");
    }
}