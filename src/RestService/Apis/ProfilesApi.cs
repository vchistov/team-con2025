namespace RestService.Apis;

using Customer.Profile.V1;
using Google.Rpc;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestService.Models;

internal static class ProfilesApi
{
    public static RouteGroupBuilder MapProfilesApiV1(this IEndpointRouteBuilder builder)
    {
        var api = builder.MapGroup("api/v{ver:apiVersion}/profiles").HasApiVersion(1.0);

        api.MapGet("/", GetAllProfilesAsync);
        api.MapGet("/{id:long}", GetProfileAsync);
        api.MapPost("/", CreateProfileAsync);

        return api;        
    }

    private static async Task<Ok<IEnumerable<ApiProfile>>> GetAllProfilesAsync([FromServices] ProfileServices services, CancellationToken cancellationToken)
    {
        var profiles = new List<ApiProfile>();

        using var streamingCall = services.ServiceClient.ListProfiles(new ListProfilesRequest(), cancellationToken: cancellationToken);
        var responseStream = streamingCall.ResponseStream;
        while (await responseStream.MoveNext(cancellationToken))
        {
            profiles.Add(responseStream.Current.ToApiProfile());
        }

        services.Logger.LogInformation("Got {ProfilesCount} profiles from gRPC service.", profiles.Count);
        return TypedResults.Ok<IEnumerable<ApiProfile>>(profiles);
    }

    private static async Task<Results<Ok<long>, Conflict<string>, ProblemHttpResult>> CreateProfileAsync(ApiCreateProfileRequest request, [FromServices] ProfileServices services, CancellationToken cancellationToken)
    {
        services.Logger.LogInformation("Create profile with parameters: {@CreationRequest}", request);

        var creationRequest = new CreateProfileRequest { FullName = request.Name, BirthDate = request.BirthDate?.ToTimestamp(), Phone = request.Phone };

        try
        {
            var response = await services.ServiceClient.CreateProfileAsync(creationRequest, cancellationToken: cancellationToken);
            services.Logger.LogInformation("Profile has been created. Id: {ProfileId}", response.Id);

            return TypedResults.Ok(response.Id);
        }
        catch (RpcException ex)
        {
            var status = ex.GetRpcStatus();

            var localizedMessage = status?.GetDetail<LocalizedMessage>();
            
            var errorInfo = status?.GetDetail<ErrorInfo>();
            if (errorInfo?.Reason == "PHONE_ALREADY_USED")
            {
                services.Logger.LogWarning("Phone is already used. Details: {@ErrorInfo}", errorInfo);
                return TypedResults.Conflict(localizedMessage?.Message ?? ex.Message);
            }

            services.Logger.LogError(ex, "Profile creation has been failed.");
            return TypedResults.Problem(detail: localizedMessage?.Message ?? ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private static async Task<Results<Ok<ApiProfile>, NotFound<string>>> GetProfileAsync(long id, [FromServices] ProfileServices services, CancellationToken cancellationToken)
    {
        try
        {
            var response = await services.ServiceClient.GetProfileAsync(new GetProfileRequest { Id = id }, cancellationToken: cancellationToken);
            services.Logger.LogInformation("Got profile: {@Profile}", response);

            return TypedResults.Ok(response.ToApiProfile());
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return TypedResults.NotFound("Profile not found.");
        }
    }
}