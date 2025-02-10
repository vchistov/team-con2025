var builder = DistributedApplication.CreateBuilder(args);

var customerProject = builder.AddProject<Projects.GrpcService>("customer").WithExternalHttpEndpoints();

var webAppProject = builder.AddProject<Projects.WebApp>("webapp").WithExternalHttpEndpoints().WithReference(customerProject);

builder.Build().Run();
