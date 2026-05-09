using GenAiAgent.Core.Models;

namespace GenAiAgent.Core.Repositories.Abstractions;

public interface ISubscriberRepository
{
    Task<IEnumerable<Subscriber>> GetAllAsync(
        CancellationToken cancellationToken);
}