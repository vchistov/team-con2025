namespace GrpcService.DataAccess;

using Bogus;
using GrpcService.DataAccess.Records;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

internal static class WebApplicationExtensions
{
    public static IServiceCollection AddDbContext(this IServiceCollection services)
    {
        // To use in-memory SQLite, it's important to understand that a new database is created whenever
        // a low-level connection is opened, and that it's deleted when that connection is closed.
        // https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database#sqlite-in-memory
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        return services.AddDbContext<DataContext>(
            opt => opt.UseSqlite(connection).EnableSensitiveDataLogging().UseSeeding((ctx, _) => SeedDatabase(ctx)));
    }

    public static void UseDbContext(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        db.Database.EnsureCreated();
    }

    private static void SeedDatabase(DbContext context)
    {
        var faker = new Faker<ProfileRecord>()
            .RuleFor(p => p.FullName, f => f.Name.FullName())
            .RuleFor(p => p.BirthDate, f => DateOnly.FromDateTime(f.Date.Past(30, DateTime.Now.AddYears(-18))))
            .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(p => p.Avatars, _ => []);

        var profiles = faker.Generate(500);
        context.Set<ProfileRecord>().AddRange(profiles);
        context.SaveChanges();

        foreach (var profile in profiles.Where((p, index) => index % 5 == 0))
        {
            var avatar = new AvatarRecord
            {
                ProfileId = profile.Id,
                Profile = profile,
                Content = GenerateRandomJpeg(32, 32),
            };
            context.Set<AvatarRecord>().Add(avatar);
        }
        context.SaveChanges();
    }

    private static byte[] GenerateRandomJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        var paint = new SKPaint
        {
            Color = new SKColor((byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(256))
        };
        canvas.DrawRect(0, 0, width, height, paint);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        return data.ToArray();
    }
}
