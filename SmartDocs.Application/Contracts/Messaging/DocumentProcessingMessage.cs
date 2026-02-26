public class DocumentProcessingMessage
{
    public Guid DocumentId { get; set; }
    public string BlobName { get; set; } = default!;
    public string FileName { get; set; } = default!;
}