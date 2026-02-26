using SmartDocs.Application.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json;
namespace SmartDocs.Infrastructure.Queue;

public class InMemoryQueueService : IQueueService
{
    private readonly List<string> _messages = new();

    public Task EnqueueAsync<T>(
        string queueName,
        T message,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);
        _messages.Add(json);

        return Task.CompletedTask;
    }
}
