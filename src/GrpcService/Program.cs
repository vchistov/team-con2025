using GrpcService.Authentication;
using GrpcService.DataAccess;
using GrpcService.Services;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddDbContext();
builder.Services
    .AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
    .AddScheme<ApiKeyOptions, ApiKeyHandler>(ApiKeyDefaults.AuthenticationScheme, opt => 
    {
        opt.ApiKey = builder.Configuration.GetValue<string>("Authentication:ApiKey")!;
    });

builder.Services.AddAuthorization(opt => opt.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(opt =>
{
    opt.SetDefaultCulture("en");
    opt.AddSupportedCultures("en", "de", "ru");
    opt.AddSupportedUICultures("en", "de", "ru");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<ProfileGrpcService>();
app.MapGrpcService<AvatarGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.UseDbContext();

app.Run();
