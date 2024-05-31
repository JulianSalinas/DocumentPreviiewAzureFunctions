using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DocumentPreviewAF.Functions;

public class GenerateSASToUpload(ILogger<GenerateSASToUpload> logger)
{
    private readonly ILogger<GenerateSASToUpload> _logger = logger;

    [Function(nameof(GenerateSASToUpload))]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        var userName = req.HttpContext?.User?.Identity?.Name ?? "Anonymous";

        _logger.LogInformation("Generate SAS for {user}", userName);

        var connString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        var blobServiceClient = new BlobServiceClient(connString);

        var blobContainerName = "original-documents";

        var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);

        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = blobContainerName,
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(60),
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

        var uri = containerClient.GenerateSasUri(sasBuilder);

        return new OkObjectResult(uri.ToString());
    }
}
