var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var blobs = storage.AddBlobs("blobs");
var queues = storage.AddQueues("queues");

builder.AddProject<Projects.ImageGallery_Web>("imagegallery-web")
       .WithReference(queues)
       .WithReference(blobs);

builder.AddAzureFunctionsProject<Projects.ImageGalleryFunctions>("imagegalleryfunctions")
       .WithReference(queues)
       .WithReference(blobs);

builder.Build().Run();
