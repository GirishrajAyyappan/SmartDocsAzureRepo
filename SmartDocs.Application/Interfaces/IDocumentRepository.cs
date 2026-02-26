using SmartDocs.Domain.Entities;

namespace SmartDocs.Application.Interfaces;

public interface IDocumentRepository
{
    Task AddAsync(Document document, CancellationToken cancellationToken);

    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task UpdateAsync(Document document, CancellationToken cancellationToken);

    Task<List<Document>> GetCompletedAsync(CancellationToken cancellationToken);
    Task<List<Document>> GetPendingAsync(CancellationToken cancellationToken);
    Task<List<Document>> GetAndMarkPendingAsync(
    int batchSize,
    CancellationToken cancellationToken);
}