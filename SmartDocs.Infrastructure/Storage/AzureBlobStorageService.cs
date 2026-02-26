using Azure.Storage.Blobs;
using SmartDocs.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace SmartDocs.Infrastructure.Storage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        var containerName = configuration["AzureStorage:BlobContainer"];
        Console.WriteLine($"Connection String: {connectionString}");
        Console.WriteLine($"Container Name: {containerName}");
        _containerClient = new BlobContainerClient(connectionString, containerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(content, overwrite: true, cancellationToken);

        return blobClient.Name;
    }
}