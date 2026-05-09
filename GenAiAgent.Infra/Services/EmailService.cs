using Microsoft.Extensions.Logging;
using GenAiAgent.Core.Services.Abstractions;

namespace GenAiAgent.Infra.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public async Task SendAsync(
        string toName, 
        string toEmail, 
        string subject, 
        string body, 
        CancellationToken cancellationToken)
    {
        await Task.Delay(150, cancellationToken);
        logger.LogInformation($"• Enviando newsletter para {toName} ({toEmail})...");
    }
}