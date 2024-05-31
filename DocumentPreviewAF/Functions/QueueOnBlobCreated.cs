using DocumentPreviewAF.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DocumentPreviewAF.Functions;

public class QueueOnBlobCreated(ILogger<QueueOnBlobCreated> logger)
{
    private readonly ILogger<QueueOnBlobCreated> _logger = logger;

    [Function(nameof(QueueOnBlobCreated))]
    [QueueOutput("documents-to-process")]
    public QueuedDocument? Run([BlobTrigger("original-documents/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name)
    {
        if (!Path.GetExtension(name).Equals(".pdf", StringComparison.CurrentCultureIgnoreCase))
        {
            _logger.LogInformation("Document ignored: {name}", name);

            return null;
        }

        _logger.LogInformation("Document queued: {name}", name);

        return new QueuedDocument
        {
            Name = name,
            Date = DateTime.UtcNow,
        };
    }
}
