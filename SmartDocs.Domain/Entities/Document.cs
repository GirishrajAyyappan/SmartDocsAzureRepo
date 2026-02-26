using SmartDocs.Domain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SmartDocs.Domain.Entities;
public class Document
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string FileName { get; set; } = default!;

    public string BlobName { get; set; } = default!;
    
    [JsonConverter(typeof(StringEnumConverter))]
    public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;

    public void MarkProcessing() => Status = DocumentStatus.Processing;

    public void MarkCompleted() => Status = DocumentStatus.Completed;

    public void MarkFailed() => Status = DocumentStatus.Failed;
}