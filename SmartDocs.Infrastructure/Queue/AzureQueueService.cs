using Azure.Storage.Queues;
using SmartDocs.Application.Interfaces;
using System.Text;
using System.Text.Json;

public class AzureQueueService : IQueueService
{
    private readonly QueueServiceClient _queueServiceClient;

    public AzureQueueService(QueueServiceClient queueServiceClient)
    {
        _queueServiceClient = queueServiceClient;
    }

    public async Task EnqueueAsync<T>(
        string queueName,
        T message,
        CancellationToken cancellationToken = default)
    {
        var queueClient = _queueServiceClient.GetQueueClient(queueName);

        await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(message);

        await queueClient.SendMessageAsync(json, cancellationToken);
    }
}
