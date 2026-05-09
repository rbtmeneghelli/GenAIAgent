using GenAiAgent.Core.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenAIAgent.Workers
{
    public class NewsletterWorker(
        ILogger<NewsletterWorker> logger,
        IServiceScopeFactory scopeFactory) : BackgroundService
    {
        private readonly TimeSpan _scheduleTime = new(8, 0, 0); // 08:00 AM

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("→ Newsletter Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = GetNextSundayAtEight(now);

                // var delay = nextRun - now;
                var delay = TimeSpan.FromSeconds(5);

                logger.LogInformation("→ Next execution scheduled for: {NextRun}", nextRun);

                try
                {
                    await Task.Delay(delay, stoppingToken);

                    logger.LogInformation("→ Executing Sunday task at: {Time}", DateTime.Now);
                    await DoWorkAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private DateTime GetNextSundayAtEight(DateTime current)
        {
            var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)current.DayOfWeek + 7) % 7;

            var nextSunday = current.Date.AddDays(daysUntilSunday).Add(_scheduleTime);

            if (nextSunday <= current)
                nextSunday = nextSunday.AddDays(7);

            return nextSunday;
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("→ Working...");
            using var scope = scopeFactory.CreateScope();
            var newsletterService = scope.ServiceProvider.GetRequiredService<INewsletterService>();
            await newsletterService.SendAsync(cancellationToken);
        }
    }
}
