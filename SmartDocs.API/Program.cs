using Microsoft.EntityFrameworkCore;
using SmartDocs.Application.Interfaces;
using SmartDocs.Application.Services;
using SmartDocs.Infrastructure.Persistence;
using SmartDocs.Infrastructure.Repositories;
using SmartDocs.Infrastructure.Storage;
using Azure.Storage.Queues;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Database
// --------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));
// --------------------
// Infrastructure Layer
// --------------------
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["AzureStorage:ConnectionString"];

    return new QueueServiceClient(connectionString);
});
builder.Services.AddSingleton<IQueueService, AzureQueueService>();


// --------------------
// Application Layer
// --------------------
builder.Services.AddScoped<DocumentService>();

// --------------------
// API
// --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();