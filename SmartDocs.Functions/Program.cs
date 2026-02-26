using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SmartDocs.Application.Interfaces;
using SmartDocs.Infrastructure.Repositories;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// REQUIRED for this template
builder.ConfigureFunctionsWebApplication();

var configuration = builder.Configuration;

var connectionString = configuration["AzureStorage:ConnectionString"];



builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

var app = builder.Build();

app.Run();