using Microsoft.EntityFrameworkCore;
using SmartDocs.Application.Interfaces;
using SmartDocs.Domain.Entities;
using SmartDocs.Domain.Enums;
using SmartDocs.Infrastructure.Persistence;

namespace SmartDocs.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<List<Document>> GetCompletedAsync(CancellationToken cancellationToken)
    {
        return await _context.Documents
            .Where(d => d.Status == DocumentStatus.Completed)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Document>> GetPendingAsync(CancellationToken cancellationToken)
    {
        return await _context.Documents
            .Where(d => d.Status == DocumentStatus.Uploaded)
            .ToListAsync(cancellationToken);
    }

public async Task<List<Document>> GetAndMarkPendingAsync(
    int batchSize,
    CancellationToken cancellationToken)
{
    var documents = await _context.Documents
        .Where(d => d.Status == DocumentStatus.Uploaded)
        .OrderBy(d => d.Id)
        .Take(batchSize)
        .ToListAsync(cancellationToken);

    if (!documents.Any())
        return documents;

    // 🔥 Immediately mark as Processing
    foreach (var document in documents)
    {
        document.MarkProcessing();
    }

    // 🔥 Persist the Processing state
    await _context.SaveChangesAsync(cancellationToken);

    return documents;
}
}