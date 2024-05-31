using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace DocumentPreviewAF.Functions;

public class ScheduleOrchestration(ILogger<ScheduleOrchestration> logger)
{
    private readonly ILogger<ScheduleOrchestration> _logger = logger;

    [Function("ScheduleOrchestration")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(ThumbnailsOrchestration));

        _logger.LogInformation("Started orchestration: {instanceId}", instanceId);

        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}
