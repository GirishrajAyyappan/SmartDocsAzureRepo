using SmartDocs.Application.Interfaces;
using SmartDocs.Domain.Entities;
using SmartDocs.Domain.Enums;

namespace SmartDocs.Application.Services;
public class DocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IQueueService _queueService;

    public DocumentService(
        IDocumentRepository repository,
        IBlobStorageService blobStorageService,
        IQueueService queueService)
    {
        _repository = repository;
        _blobStorageService = blobStorageService;
        _queueService = queueService;
    }

public async Task<Guid> UploadDocumentAsync(
    Stream fileStream,
    string fileName,
    string contentType,
    CancellationToken cancellationToken = default)
{
    var id = Guid.NewGuid(); 

    var blobName = await _blobStorageService.UploadAsync(
        fileStream,
        fileName,
        contentType,
        cancellationToken);

    var document = new Document
    {
        Id = id.ToString(),
        FileName = fileName,
        BlobName = blobName,
        Status = DocumentStatus.Uploaded
    };

    await _repository.AddAsync(document, cancellationToken);

    var message = new DocumentProcessingMessage
    {
        DocumentId = id,
        BlobName = blobName,
        FileName = fileName
    };

    await _queueService.EnqueueAsync(
        StorageConstants.ProcessingQueue,
        message,
        cancellationToken);

    return id;
}
    public async Task<Document?> GetDocumentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }
}