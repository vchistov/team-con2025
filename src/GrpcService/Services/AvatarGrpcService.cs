namespace GrpcService.Services;

using Customer.Avatar.V1;
using Grpc.Core;
using GrpcService.DataAccess;
using GrpcService.DataAccess.Records;
using GrpcService.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

[Authorize]
internal class AvatarGrpcService(DataContext dataContext, ILogger<AvatarGrpcService> logger) : AvatarService.AvatarServiceBase
{
    public override async Task<AddAvatarResponse> AddAvatar(AddAvatarRequest request, ServerCallContext context)
    {
        var profile = await dataContext.Profiles.FirstOrDefaultAsync(p => p.Id == request.ProfileId, context.CancellationToken);
        if (profile is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Profile not found."));
        }

        var avatarRecord = new AvatarRecord
        { 
            Profile = profile,
            ProfileId = profile.Id,
            Content = request.Content.ToByteArray()
        };

        dataContext.Avatars.Add(avatarRecord);
        await dataContext.SaveChangesAsync(context.CancellationToken);

        return new AddAvatarResponse { Id = avatarRecord.Id };
    }

    public override async Task<AddRandomAvatarResponse> AddRandomAvatar(AddRandomAvatarRequest request, ServerCallContext context)
    {
        var profile = await dataContext.Profiles.FirstOrDefaultAsync(p => p.Id == request.ProfileId, context.CancellationToken);
        if (profile is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Profile not found."));
        }

        var avatarRecord = new AvatarRecord
        {
            Profile = profile,
            ProfileId = profile.Id,
            Content = ImageGenerator.GenerateRandomJpeg(128, 128)
        };

        dataContext.Avatars.Add(avatarRecord);
        await dataContext.SaveChangesAsync(context.CancellationToken);

        return new AddRandomAvatarResponse { Id = avatarRecord.Id };
    }

    public override async Task<DeleteAvatarResponse> DeleteAvatar(DeleteAvatarRequest request, ServerCallContext context)
    {
        var avatar = await dataContext.Avatars.FirstOrDefaultAsync(a => a.Id == request.Id && a.ProfileId == request.ProfileId, context.CancellationToken);
        if (avatar is null)
        {
            return new DeleteAvatarResponse { Success = false };
        }

        dataContext.Avatars.Remove(avatar);
        await dataContext.SaveChangesAsync(context.CancellationToken);
        return new DeleteAvatarResponse { Success = true };
    }

    public override async Task<Avatar> GetAvatar(GetAvatarRequest request, ServerCallContext context)
    {
        if (request.ProfileId == 4 || request.ProfileId == 5)
        {
            // That's for training purposes, the long-running operation
            await Task.Delay(TimeSpan.FromSeconds(30), context.CancellationToken);
        }

        if (request.ProfileId == 6)
        {
            // That's for training purposes, when request id is 6, the request always fails with Unavailable code.
            throw new RpcException(new Status(StatusCode.Unavailable, "Permanently out of service."));
        }

        var avatar = await dataContext.Avatars.AsNoTracking().FirstOrDefaultAsync(a => a.Id == request.Id && a.ProfileId == request.ProfileId, context.CancellationToken);
        if (avatar is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Avatar not found."));
        }

        return avatar.ToAvatar();
    }

    public override async Task ListAvatars(ListAvatarsRequest request, IServerStreamWriter<Avatar> responseStream, ServerCallContext context)
    {
        await foreach (var avatar in dataContext.Avatars.Where(a => a.ProfileId == request.ProfileId).AsNoTracking().AsAsyncEnumerable())
        {
            logger.LogInformation("Send to stream avatar #{AvatarId} of profile #{ProfileId}.", avatar.Id, avatar.ProfileId);
            await responseStream.WriteAsync(avatar.ToAvatar(), context.CancellationToken);
        }
    }
}