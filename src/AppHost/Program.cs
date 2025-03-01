var builder = DistributedApplication.CreateBuilder(args);

var grpcProject = builder.AddProject<Projects.GrpcService>("grpc-svc").WithExternalHttpEndpoints();

// var webAppProject = builder.AddProject<Projects.WebApp>("webapp").WithExternalHttpEndpoints().WithReference(grpcProject);

var restProject = builder.AddProject<Projects.RestService>("rest-svc").WithExternalHttpEndpoints().WithReference(grpcProject);

builder.Build().Run();
