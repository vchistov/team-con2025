namespace GrpcService.Services;

using Customer.Profile.V1;
using Grpc.Core;
using GrpcService.DataAccess;
using GrpcService.DataAccess.Records;
using Microsoft.EntityFrameworkCore;

internal sealed class ProfileGrpcService(DataContext dataContext) : ProfileService.ProfileServiceBase
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

        await dataContext.Profiles.AddAsync(record, context.CancellationToken);
        await dataContext.SaveChangesAsync(context.CancellationToken);

        return new CreateProfileResponse { Id = record.Id };
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
        var profile = await dataContext.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.Id, context.CancellationToken);
        if (profile is null)
        {
            context.Status = new Status(StatusCode.NotFound, "Profile not found.");

            // That's by design in gRPC .net, the method must to return null, it should be object, otherwise RpcException
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
