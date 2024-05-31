using Azure.Storage.Queues.Models;
using DocumentPreviewAF.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;


namespace DocumentPreviewAF.Functions;

public class ScheduleOnBlobQueued(ILogger<ScheduleOnBlobQueued> logger)
{
    private readonly ILogger<ScheduleOnBlobQueued> _logger = logger;

    [Function(nameof(ScheduleOnBlobQueued))]
    public async Task<string> Run(
        [QueueTrigger("documents-to-process", Connection = "AzureWebJobsStorage")] QueueMessage message,
        [DurableClient] DurableTaskClient client)
    {
        var document = message.Body.ToObjectFromJson<QueuedDocument>();

        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(ThumbnailsOrchestration), document);

        _logger.LogInformation("Started orchestration: {instanceId}", instanceId);

        return instanceId;
    }
}
