using GenAiAgent.Core.Models;

namespace GenAiAgent.Core.Repositories.Abstractions;

public interface IArticleRepository
{
    Task<IEnumerable<Article>> GetFromLastWeekAsync(
        CancellationToken cancellationToken);
}