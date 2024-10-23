using Azure.Storage.Queues;
using System.Text.Json;

namespace ImageGallery.Web;

public class QueueWorker(QueueServiceClient queueServiceClient, QueueMessageHandler handler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var queueClient = queueServiceClient.GetQueueClient("thumbnailresults");
            await queueClient.CreateIfNotExistsAsync();
            var message = await queueClient.ReceiveMessageAsync(TimeSpan.FromSeconds(1), stoppingToken);
            if (message is not null && message.Value is not null)
            {
                UploadResult? result = JsonSerializer.Deserialize<UploadResult>(message.Value.Body.ToString());
                if(result is not null)
                {
                    handler.OnMessageReceived(result);
                    await queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
                }
            }

            await Task.Delay(1000);
        }
    }
}

public class QueueMessageHandler
{
    public event EventHandler<UploadResult>? MessageReceived;

    public void OnMessageReceived(UploadResult result)
    {
        if (MessageReceived is not null)
            MessageReceived(this, result);
    }
}

public class UploadResult
{
}