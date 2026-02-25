using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartDocs.Application.Interfaces;

namespace SmartDocs.Functions;

public class DocumentProcessorFunction
{
    private readonly ILogger<DocumentProcessorFunction> _logger;
    private readonly IDocumentRepository _repository;

   public DocumentProcessorFunction(
    ILogger<DocumentProcessorFunction> logger,
    IDocumentRepository repository)
{
    _logger = logger;
    _repository = repository;
}

    [Function("DocumentProcessorFunction")]
    public async Task Run(
        [QueueTrigger("smartdocs-queue", Connection = "StorageConnection")]
        string documentId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Received document ID: {DocumentId}", documentId);

            var id = Guid.Parse(documentId);

            var document = await _repository.GetByIdAsync(id, cancellationToken);

            if (document == null)
            {
                _logger.LogWarning("Document not found in DB");
                return;
            }

            document.MarkProcessing();
            await _repository.UpdateAsync(document, cancellationToken);

            _logger.LogInformation("Processing document {DocumentId}", documentId);

            await Task.Delay(2000, cancellationToken);

            document.MarkCompleted();
            await _repository.UpdateAsync(document, cancellationToken);

            _logger.LogInformation("Completed document {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processing failed");
            throw; // important for retry
        }
    }
}