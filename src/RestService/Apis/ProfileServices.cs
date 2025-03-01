namespace RestService.Apis;

using Customer.Profile.V1;
using Microsoft.AspNetCore.Mvc;

public class ProfileServices(
    [FromServices] ProfileService.ProfileServiceClient serviceClient,
    ILogger<ProfileServices> logger)
{
    public ProfileService.ProfileServiceClient ServiceClient { get; } = serviceClient;
    public ILogger<ProfileServices> Logger { get; } = logger;
}
