using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ImageGalleryFunctions;

public class ThumbnailGenerator
{
    private readonly ILogger<ThumbnailGenerator> _logger;

    public ThumbnailGenerator(ILogger<ThumbnailGenerator> logger)
    {
        _logger = logger;
    }

    [Function("ThumbnailGenerator")]
    public async Task Run([BlobTrigger("images/{name}", Connection = "blobs")] Stream stream, string name)
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
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating thumbnail for image: {name}. Exception: {ex.Message}");
        }
    }
}
