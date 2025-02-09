var builder = DistributedApplication.CreateBuilder(args);

var customerService = builder.AddProject<Projects.GrpcService>("customer").WithExternalHttpEndpoints();

builder.Build().Run();
