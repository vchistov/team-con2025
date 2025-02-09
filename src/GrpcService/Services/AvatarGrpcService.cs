namespace GrpcService.Services;

using Customer.Avatar.V1;
using Grpc.Core;
using GrpcService.DataAccess;
using GrpcService.DataAccess.Records;
using Microsoft.EntityFrameworkCore;

internal class AvatarGrpcService(DataContext dataContext) : AvatarService.AvatarServiceBase
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
            Content = request.ImageData.ToByteArray()
        };

        dataContext.Avatars.Add(avatarRecord);
        await dataContext.SaveChangesAsync(context.CancellationToken);

        return new AddAvatarResponse { Id = avatarRecord.Id };
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
        var avatar = await dataContext.Avatars.AsNoTracking().FirstOrDefaultAsync(a => a.Id == request.Id && a.ProfileId == request.ProfileId, context.CancellationToken);
        if (avatar is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Avatar not found."));
        }

        return avatar.ToAvatar();
    }
}
