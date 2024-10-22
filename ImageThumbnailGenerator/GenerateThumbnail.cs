using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ImageThumbnailGenerator
{
    public class GenerateThumbnail
    {
        private readonly ILogger<GenerateThumbnail> _logger;

        public GenerateThumbnail(ILogger<GenerateThumbnail> logger)
        {
            _logger = logger;
        }

        [Function("GenerateThumbnail")]
        public async Task Run([BlobTrigger("images/{name}", Connection = "blobs")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
        }
    }
}
