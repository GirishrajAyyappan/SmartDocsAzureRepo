using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartDocs.Application.Interfaces;
using SmartDocs.Domain.Enums;

public class ReportingJob : BackgroundService
{
    private readonly IDocumentRepository _repository;
    private readonly ILogger<ReportingJob> _logger;
    private readonly TimeSpan _interval;

    public ReportingJob(
        IDocumentRepository repository,
        ILogger<ReportingJob> logger,
        IConfiguration configuration)
    {
        _repository = repository;
        _logger = logger;

        var intervalSeconds =
            configuration.GetValue<int>("WebJobSettings:IntervalSeconds");

        if (intervalSeconds <= 0)
        {
            intervalSeconds = 10;
        }

        _interval = TimeSpan.FromSeconds(intervalSeconds);
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReportingJob started at {time}",
            DateTime.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 🔥 Atomic Fetch + MarkProcessing
                var pendingDocuments =
                    await _repository.GetAndMarkPendingAsync(
                        5,
                        stoppingToken);

                _logger.LogInformation(
                    "[WebJob] {time}: Pending Documents Count: {count}",
                    DateTime.Now,
                    pendingDocuments.Count);

                foreach (var doc in pendingDocuments)
                {
                    try
                    {
                        _logger.LogInformation(
                            "Processing Document Id: {id}",
                            doc.Id);

                        // Simulate actual work
                        await Task.Delay(1000, stoppingToken);

                        doc.MarkCompleted();

                        await _repository.UpdateAsync(
                            doc,
                            stoppingToken);

                        _logger.LogInformation(
                            "Document {id} marked as Completed",
                            doc.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error processing Document {id}",
                            doc.Id);

                        doc.MarkFailed();

                        await _repository.UpdateAsync(
                            doc,
                            stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Critical error in ReportingJob loop");
            }

            // ⏳ Wait before next poll
            await Task.Delay(_interval, stoppingToken);
        }
    }
}