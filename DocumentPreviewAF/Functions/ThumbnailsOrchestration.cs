using Azure.Storage.Blobs;
using DocumentPreviewAF.Models;
using DocumentPreviewAF.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DocumentPreviewAF.Functions;

public static class ThumbnailsOrchestration
{
    [Function(nameof(ThumbnailsOrchestration))]
    public static async Task RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(ThumbnailsOrchestration));

        var document = context.GetInput<QueuedDocument>();

        logger.LogInformation("ThumbnailsOrchestration Started, Document: {name}", document!.Name);

        var tasks = new List<Task>
        {
            context.CallActivityAsync<string>(nameof(GenerateThumbnail), new ThumbnailOptions
            {
                Name = document.Name,
                ImageSize = ImageSize.ExtraSmall,
            }),

            context.CallActivityAsync<string>(nameof(GenerateThumbnail), new ThumbnailOptions
            {
                Name = document.Name,
                ImageSize = ImageSize.Small,
            }),

            context.CallActivityAsync<string>(nameof(GenerateThumbnail), new ThumbnailOptions
            {
                Name = document.Name,
                ImageSize = ImageSize.Medium,
            })
        };

        await Task.WhenAll(tasks);
    }

    [Function(nameof(GenerateThumbnail))]
    public static async Task GenerateThumbnail(
        [ActivityTrigger] ThumbnailOptions options,
        [BlobInput("original-documents")] BlobContainerClient sourceContainerClient,
        [BlobInput("preview-documents")] BlobContainerClient targetContainerClient,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger(nameof(GenerateThumbnail));

        logger.LogInformation("Generating thumbnail, options: ", options);

        var converter = new PdfToThumbnail();

        var blobClient = sourceContainerClient.GetBlobClient(options.Name);

        var blobStream = blobClient.OpenRead();

        var imageStream = converter.Convert(blobStream, options.ImageSize);

        var targetBlobName = $"{options.ImageSize.GetPrefix()}/{options}.png";

        var targetBlobClient = targetContainerClient.GetBlobClient(targetBlobName);

        await targetBlobClient.UploadAsync(imageStream);

        logger.LogInformation($"Thumbnail generated, {targetBlobName} at {DateTime.Now}");
    }
}
