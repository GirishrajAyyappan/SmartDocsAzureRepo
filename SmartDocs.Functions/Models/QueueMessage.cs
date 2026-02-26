namespace SmartDocs.Functions.Models;

public class QueueMessage
{
    public string DocumentId { get; set; } = default!;
    public string BlobName { get; set; } = default!;
    public string FileName { get; set; } = default!;
}