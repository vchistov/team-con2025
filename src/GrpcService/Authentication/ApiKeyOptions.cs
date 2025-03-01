namespace GrpcService.Authentication;

using Microsoft.AspNetCore.Authentication;

public class ApiKeyOptions : AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = "X-Api-Key";

    public string ApiKey { get; set; } = default!;
}
