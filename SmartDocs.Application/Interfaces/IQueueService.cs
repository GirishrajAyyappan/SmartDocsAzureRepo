namespace SmartDocs.Application.Interfaces;

public interface IQueueService
{
    Task EnqueueAsync(Guid documentId);
    Task<Guid?> DequeueAsync();
}
