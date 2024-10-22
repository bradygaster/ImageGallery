var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ImageGallery_Web>("imagegallery-web");

builder.Build().Run();
