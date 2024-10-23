using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;

namespace ImageGalleryFunctions;

public class ThumbnailGenerator
{
    private readonly ILogger<ThumbnailGenerator> _logger;
    private readonly QueueServiceClient queueServiceClient;

    public ThumbnailGenerator(ILogger<ThumbnailGenerator> logger,
        QueueServiceClient queueServiceClient)
    {
        _logger = logger;
        this.queueServiceClient = queueServiceClient;
    }

    public class UploadResult
    {
    }

    [Function("ThumbnailGenerator")]
    public async Task Run([BlobTrigger("images/{name}", Connection = "blobs")] Stream stream, 
        string name)
    {
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("blobs");
            var blobServiceClient = new BlobServiceClient(connectionString);
            using var image = await Image.LoadAsync(stream);

            int maxHeight = 128;
            var scale = (double)maxHeight / image.Height;
            int thumbnailWidth = (int)(image.Width * scale);
            int thumbnailHeight = (int)(image.Height * scale);

            image.Mutate(x => x.Resize(thumbnailWidth, thumbnailHeight));
            var containerClient = blobServiceClient.GetBlobContainerClient("thumbnails");
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(name);

            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new JpegEncoder());
            outputStream.Position = 0;

            await blobClient.UploadAsync(outputStream, overwrite: true);

            var resultsQueueClient = queueServiceClient.GetQueueClient("thumbnailresults");
            await resultsQueueClient.CreateIfNotExistsAsync();

            await resultsQueueClient.SendMessageAsync(JsonSerializer.Serialize(new UploadResult()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating thumbnail for image: {name}. Exception: {ex.Message}");
        }
    }
}
