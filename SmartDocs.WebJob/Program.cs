using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartDocs.Application.Interfaces;
using SmartDocs.Domain.Entities;
using SmartDocs.Infrastructure.Repositories;
using System.Text.Json.Serialization;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
 

        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // Necessary services
        services.AddHostedService<ReportingJob>();
    })
    .Build();

await host.RunAsync();