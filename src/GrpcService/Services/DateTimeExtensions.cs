using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GrpcService.Services;

internal static class DateTimeExtensions
{
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
