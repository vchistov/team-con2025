namespace GrpcService.DataAccess;

using Bogus;
using GrpcService.DataAccess.Records;
using GrpcService.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

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
        var bytes = RandomNumberGenerator.GetBytes(32);

        var faker = new Faker<ProfileRecord>()
            .RuleFor(p => p.FullName, f => f.Name.FullName())
            .RuleFor(p => p.BirthDate, f => Random.Shared.Next(100) > 50 ? DateOnly.FromDateTime(f.Date.Past(30, DateTime.Now.AddYears(-18))) : null)
            .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber("###-###-####"))
            .RuleFor(p => p.Avatars, _ => []);

        var profiles = faker.Generate(50);
        context.Set<ProfileRecord>().AddRange(profiles);
        context.SaveChanges();

        foreach (var profile in profiles.Where((p, index) => index % 5 == 0))
        {
            var avatar = new AvatarRecord
            {
                ProfileId = profile.Id,
                Profile = profile,
                Content = ImageGenerator.GenerateRandomJpeg(64, 64),
            };
            context.Set<AvatarRecord>().Add(avatar);
        }

        context.SaveChanges();
    }
}
