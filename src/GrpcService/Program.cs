using GrpcService.DataAccess;
using GrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddDbContext();
builder.AddServiceDefaults();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}
app.MapGrpcService<ProfileGrpcService>();
app.MapGrpcService<AvatarGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.UseDbContext();

app.Run();
