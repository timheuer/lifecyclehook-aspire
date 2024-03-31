var builder = DistributedApplication.CreateBuilder(args);

builder.AddCustomResource("garnet");

builder.Build().Run();
