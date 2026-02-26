using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SmartDocs.Application.Interfaces;
using System.Text.Json;
using SmartDocs.Functions.Models;

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
    [QueueTrigger("queue-smartdocs-dev", Connection = "StorageConnection")]
    QueueMessage message,
    CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
         "Processing DocumentId: {DocumentId}, Blob: {BlobName}",
         message.DocumentId,
         message.BlobName);


            if (message == null)
            {
                _logger.LogError("Failed to deserialize message");
                return;
            }

            var id = Guid.Parse(message.DocumentId);

            var document = await _repository.GetByIdAsync(id, cancellationToken);

            if (document == null)
            {
                _logger.LogWarning("Document not found in DB");
                return;
            }

            document.MarkProcessing();
            await _repository.UpdateAsync(document, cancellationToken);

            await Task.Delay(2000, cancellationToken);

            document.MarkCompleted();
            await _repository.UpdateAsync(document, cancellationToken);

            _logger.LogInformation("Completed document {DocumentId}", message.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processing failed");
            throw;
        }
    }
}