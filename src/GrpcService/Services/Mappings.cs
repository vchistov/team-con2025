namespace GrpcService.Services;

using Customer.Avatar.V1;
using Customer.Profile.V1;
using Google.Protobuf;
using GrpcService.DataAccess.Records;

internal static class Mappings
{
    public static Profile ToProfile(this ProfileRecord record)
    {
        return new Profile
        {
            Id = record.Id,
            FullName = record.FullName,
            BirthDate = record.BirthDate?.ToTimestamp(),
            Phone = record.Phone
        };
    }

    public static Avatar ToAvatar(this AvatarRecord record)
    {
        return new Avatar
        {
            Id = record.Id,
            ProfileId = record.ProfileId,
            Content = ByteString.CopyFrom(record.Content)
        };
    }
}
