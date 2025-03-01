namespace RestService.Models;

using Customer.Profile.V1;
using Google.Protobuf.WellKnownTypes;

public static class Mappings
{
    public static ApiProfile ToApiProfile(this Profile profile)
    {
        return new ApiProfile
        {
            Id = profile.Id,
            Name = profile.FullName,
            BirthDate = profile.BirthDate != null ? DateOnly.FromDateTime(profile.BirthDate.ToDateTime()) : null,
            Phone = profile.Phone
        };
    }

    public static Timestamp ToTimestamp(this DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Local => Timestamp.FromDateTime(dateTime.ToUniversalTime()),
            DateTimeKind.Utc => Timestamp.FromDateTime(dateTime),
            _ => Timestamp.FromDateTime(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc))
        };
    }

    public static Timestamp ToTimestamp(this DateOnly date) => date.ToDateTime(TimeOnly.MinValue).ToTimestamp();

    public static DateOnly ToDateOnly(this Timestamp timestamp) => DateOnly.FromDateTime(timestamp.ToDateTime());
}
