namespace GrpcService.Services;

using Customer.Profile.V1;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using GrpcService.DataAccess;
using GrpcService.DataAccess.Records;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;

[Authorize]
internal sealed class ProfileGrpcService(DataContext dataContext, IStringLocalizer<ProfileGrpcService> localizer) : ProfileService.ProfileServiceBase
{
    private static Profile Empty = new();

    public override async Task<CreateProfileResponse> CreateProfile(CreateProfileRequest request, ServerCallContext context)
    {
        var record = new ProfileRecord
        {
            FullName = request.FullName,
            BirthDate = request.BirthDate?.ToDateOnly(),
            Phone = request.Phone,
        };

        try
        {
            await dataContext.Profiles.AddAsync(record, context.CancellationToken);
            await dataContext.SaveChangesAsync(context.CancellationToken);

            return new CreateProfileResponse { Id = record.Id };
        }
        catch (DbUpdateException ex) when (ex.IsUniqueIndexViolation())
        {
            var errorInfo = new ErrorInfo { Domain = "customer", Reason = "PHONE_ALREADY_USED" };
            errorInfo.Metadata.Add("phone", request.Phone);

            var status = new Google.Rpc.Status
            {
                Code = (int)Code.AlreadyExists,
                Message = "Phone already used by another profile.",
                Details =
                {
                    Any.Pack(errorInfo),
                    Any.Pack(new LocalizedMessage { Locale=CultureInfo.CurrentUICulture.Name, Message=localizer["PhoneAlreadyUsed", request.Phone] }),
                }
            };
            throw status.ToRpcException();
        }
    }

    public override async Task<DeleteProfileResponse> DeleteProfile(DeleteProfileRequest request, ServerCallContext context)
    {
        var profile = await dataContext.Profiles.FirstOrDefaultAsync(p => p.Id == request.Id, context.CancellationToken);
        if (profile is null)
        {
            return new DeleteProfileResponse { Success = false };
        }

        dataContext.Profiles.Remove(profile);
        await dataContext.SaveChangesAsync(context.CancellationToken);
        return new DeleteProfileResponse { Success = true };
    }

    public override async Task<Profile> GetProfile(GetProfileRequest request, ServerCallContext context)
    {
        if (request.Id == 5)
        {
            var attemptCountEntry = context.RequestHeaders.Get("grpc-previous-rpc-attempts");
            if (attemptCountEntry == null || (int.TryParse(attemptCountEntry.Value, out var attemptCount) && attemptCount < 2))
            {
                // That's for training purposes, when request id is 5, the request fails on first and second invocations.
                throw new RpcException(new Grpc.Core.Status(StatusCode.Unavailable, "Temporarily out of service."));
            }
        }
        else if (request.Id == 6)
        {
            // That's for training purposes, when request id is 6, the request always fails with Unavailable code.
            throw new RpcException(new Grpc.Core.Status(StatusCode.Unavailable, "Permanently out of service."));
        }

        var profile = await dataContext.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.Id, context.CancellationToken);
        if (profile is null)
        {
            context.Status = new Grpc.Core.Status(StatusCode.NotFound, "Profile not found.");

            // That's by design in gRPC .net, the method must not return null, it should be object, otherwise RpcException
            // https://github.com/grpc/grpc-dotnet/issues/1764
            // https://github.com/grpc/grpc-dotnet/issues/1555
            return Empty;
        }

        return profile.ToProfile();
    }

    public override async Task ListProfiles(ListProfilesRequest request, IServerStreamWriter<Profile> responseStream, ServerCallContext context)
    {
        await foreach (var profile in dataContext.Profiles.AsNoTracking().AsAsyncEnumerable())
        {
            await responseStream.WriteAsync(profile.ToProfile(), context.CancellationToken);
        }
    }
}
