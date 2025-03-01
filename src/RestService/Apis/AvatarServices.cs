namespace RestService.Apis;

using Customer.Avatar.V1;
using Microsoft.AspNetCore.Mvc;

public class AvatarServices(
    [FromServices] AvatarService.AvatarServiceClient serviceClient,
    ILogger<AvatarServices> logger)
{
    public AvatarService.AvatarServiceClient ServiceClient { get; } = serviceClient;
    public ILogger<AvatarServices> Logger { get; } = logger;
}
