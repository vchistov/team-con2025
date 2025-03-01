using Customer.Avatar.V1;
using Customer.Profile.V1;
using Microsoft.AspNetCore.Http.Features;
using RestService.Apis;
using RestService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddAntiforgery();

builder.AddGrpcClient<ProfileService.ProfileServiceClient>();
builder.AddGrpcClient<AvatarService.AvatarServiceClient>();

builder.Services.AddSingleton<ProfileServices>();
builder.Services.AddSingleton<AvatarServices>();

builder.Services.AddApiVersioning();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAntiforgery();

app.MapDefaultEndpoints();

var api = app.NewVersionedApi("Customers");
api.MapProfilesApiV1();
api.MapAvatarsApiV1();

app.Run();