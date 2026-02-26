using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using SmartDocs.Application.Interfaces;
using SmartDocs.Domain.Entities;
using SmartDocs.Domain.Enums;
using CosmosContainer = Microsoft.Azure.Cosmos.Container;

namespace SmartDocs.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly CosmosContainer _container;

    public DocumentRepository(IConfiguration configuration)
    {
        var connectionString = configuration["CosmosDb:ConnectionString"];
        var databaseName = configuration["CosmosDb:DatabaseName"];
        var containerName = configuration["CosmosDb:ContainerName"];

        var client = new CosmosClient(connectionString);
        _container = client.GetContainer(databaseName, containerName);
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken)
    {
        await _container.CreateItemAsync(document, new PartitionKey(document.Id), cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken)
    {
        await _container.UpsertItemAsync(document, new PartitionKey(document.Id), cancellationToken: cancellationToken);
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<Document>(
                id.ToString(),
                new PartitionKey(id.ToString()),
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<Document>> GetCompletedAsync(CancellationToken cancellationToken)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.Status = @status")
            .WithParameter("@status", DocumentStatus.Completed.ToString());

        return await ExecuteQueryAsync(query, cancellationToken);
    }

    public async Task<List<Document>> GetPendingAsync(CancellationToken cancellationToken)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.Status = @status")
            .WithParameter("@status", DocumentStatus.Uploaded.ToString());

        return await ExecuteQueryAsync(query, cancellationToken);
    }

    public async Task<List<Document>> GetAndMarkPendingAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.Status = @status ORDER BY c.id")
            .WithParameter("@status", DocumentStatus.Uploaded.ToString());

        var documents = new List<Document>();
        var iterator = _container.GetItemQueryIterator<Document>(query);

        while (iterator.HasMoreResults && documents.Count < batchSize)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            documents.AddRange(response);
        }

        documents = documents.Take(batchSize).ToList();

        foreach (var document in documents)
        {
            document.MarkProcessing();
            await _container.UpsertItemAsync(document, new PartitionKey(document.Id), cancellationToken: cancellationToken);
        }

        return documents;
    }

    private async Task<List<Document>> ExecuteQueryAsync(
        QueryDefinition query,
        CancellationToken cancellationToken)
    {
        var results = new List<Document>();
        var iterator = _container.GetItemQueryIterator<Document>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }
}