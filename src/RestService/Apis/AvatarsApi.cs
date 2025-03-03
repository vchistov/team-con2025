namespace RestService.Apis;

using Customer.Avatar.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

public static class AvatarsApi
{
    public static RouteGroupBuilder MapAvatarsApiV1(this IEndpointRouteBuilder builder)
    {
        var api = builder.MapGroup("api/v{ver:apiVersion}/profiles/{profileId:long}/avatars").HasApiVersion(1.0);

        api.MapGet("/{id:long}", GetAvatarAsync);
        api.MapGet("/_first", GetFirstAvatarAsync);
        api.MapGet("/_upload", GetAvatarUploadFormAsync);
        api.MapPost("/_upload", AddAvatarAsync);
        api.MapPost("/_random", AddRandomAvatarAsync);
        api.MapDelete("/{id:long}", DeleteAvatarAsync);

        return api;
    }

    private static async Task<Results<FileContentHttpResult, NotFound<string>>> GetAvatarAsync(long profileId, long id, [FromServices] AvatarServices services, CancellationToken cancellationToken)
    {
        try
        {
            /*
             For training purposes on gRPC service side it emulates long-running request for profiles #4 and #5.
             Also it emulates 'Unavailable' error for profile #6.
             */

            DateTime? deadline = null;
            if (profileId == 4)
            {
                // 5 seconds timeout for request avatar of profile #4.
                deadline = DateTime.UtcNow.AddSeconds(5);
            }

            var cts = new CancellationTokenSource();
            if (profileId == 5)
            {
                // request cancellation after 5 seconds for avatar of profile #5.
                cts.CancelAfter(TimeSpan.FromSeconds(5));
                cancellationToken.Register(cts.Cancel);
            }

            var response = await services.ServiceClient.GetAvatarAsync(new GetAvatarRequest { ProfileId = profileId, Id = id }, deadline: deadline, cancellationToken: cts.Token);
            return TypedResults.File(response.Content.ToByteArray(), contentType: MediaTypeNames.Image.Jpeg);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return TypedResults.NotFound("Avatar not found.");
        }
    }

    private static async Task<Results<FileContentHttpResult, NotFound<string>>> GetFirstAvatarAsync(long profileId, [FromServices] AvatarServices services, HttpContext httpContext, CancellationToken cancellationToken)
    {
        using var streamingCall = services.ServiceClient.ListAvatars(new ListAvatarsRequest { ProfileId = profileId }, cancellationToken: cancellationToken);
        var responseStream = streamingCall.ResponseStream;
        if (await responseStream.MoveNext(cancellationToken))
        {
            httpContext.Response.Headers["X-Profile-AvatarId"] = responseStream.Current.Id.ToString();
            return TypedResults.File(responseStream.Current.Content.ToByteArray(), contentType: MediaTypeNames.Image.Jpeg);
        }

        return TypedResults.NotFound("Avatar not found.");
    }

    private static ContentHttpResult GetAvatarUploadFormAsync(long profileId, [FromServices] AvatarServices services, HttpContext httpContext, IAntiforgery antiforgery, CancellationToken cancellationToken)
    {
        var token = antiforgery.GetAndStoreTokens(httpContext);

        var html = $"""
          <html>
            <body>
              <form action="{httpContext.Request.Path}" method="POST" enctype="multipart/form-data">
                <input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken!}" required/>
                <input type="file" name="imageData" placeholder="Upload an image..." accept=".jpg, .jpeg, .png" />
                <input type="submit" />
              </form> 
            </body>
          </html>
        """;

        return TypedResults.Content(html, MediaTypeNames.Text.Html);
    }

    private static async Task<Ok<long>> AddAvatarAsync(long profileId, IFormFile imageData, [FromServices] AvatarServices services, CancellationToken cancellationToken)
    {
        var response = await services.ServiceClient.AddAvatarAsync(new AddAvatarRequest { ProfileId = profileId, Content = ByteString.FromStream(imageData.OpenReadStream()) }, cancellationToken: cancellationToken);
        return TypedResults.Ok(response.Id);
    }

    private static async Task<Ok<long>> AddRandomAvatarAsync(long profileId, [FromServices] AvatarServices services, CancellationToken cancellationToken)
    {
        var response = await services.ServiceClient.AddRandomAvatarAsync(new AddRandomAvatarRequest { ProfileId = profileId }, cancellationToken: cancellationToken);
        return TypedResults.Ok(response.Id);
    }

    private static async Task<Ok<bool>> DeleteAvatarAsync(long profileId, long id, [FromServices] AvatarServices services, CancellationToken cancellationToken)
    {
        var response = await services.ServiceClient.DeleteAvatarAsync(new DeleteAvatarRequest { ProfileId = profileId, Id = id }, cancellationToken: cancellationToken);
        return TypedResults.Ok(response.Success);
    }
}