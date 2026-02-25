using SmartDocs.Domain.Enums;

namespace SmartDocs.Domain.Entities;

public class Document
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; }
    public string BlobName { get; private set; }
    public DocumentStatus Status { get; private set; }

    public Document(Guid id, string fileName, string blobName)
    {
        Id = id;
        FileName = fileName;
        BlobName = blobName;
        Status = DocumentStatus.Uploaded;
    }

   public void MarkProcessing()
{
    if (Status != DocumentStatus.Uploaded)
        throw new InvalidOperationException("Only uploaded documents can move to processing.");

    Status = DocumentStatus.Processing;
}

public void MarkCompleted()
{
    if (Status != DocumentStatus.Processing)
        throw new InvalidOperationException("Only processing documents can be completed.");

    Status = DocumentStatus.Completed;
}

public void MarkFailed()
{
    if (Status != DocumentStatus.Processing)
        throw new InvalidOperationException("Only processing documents can fail.");

    Status = DocumentStatus.Failed;
}

}