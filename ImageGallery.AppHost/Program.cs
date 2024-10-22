var builder = DistributedApplication.CreateBuilder(args);
var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var blobs = storage.AddBlobs("blobs");

builder.AddProject<Projects.ImageGallery_Web>("imagegallery-web")
       .WithReference(blobs);

builder.AddAzureFunctionsProject<Projects.ImageGalleryFunctions>("imagegalleryfunctions")
       .WithReference(blobs);

builder.Build().Run();
