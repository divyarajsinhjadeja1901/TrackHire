using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrackHire.API.Data;
using TrackHire.API.Helpers;
using TrackHire.API.Models;

namespace TrackHire.Tests;

internal static class TestHelpers
{
    public const string JwtKey = "unit-test-signing-key-that-is-at-least-32-chars-long!";

    public static IConfiguration CreateConfig(string key = JwtKey) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = "TrackHireAPI",
                ["Jwt:Audience"] = "TrackHireClient"
            })
            .Build();

    public static JwtHelper CreateJwtHelper(string key = JwtKey) => new(CreateConfig(key));

    // Each test gets its own isolated in-memory database.
    public static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    public static User SeedUser(AppDbContext db, int id, string email)
    {
        var user = new User { Id = id, Name = $"User {id}", Email = email, PasswordHash = "hash" };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public static JobApplication SeedApplication(
        AppDbContext db, int userId, string company = "Acme", string status = "Applied", DateTime? appliedDate = null)
    {
        var app = new JobApplication
        {
            UserId = userId,
            CompanyName = company,
            JobTitle = "Developer",
            Status = status,
            AppliedDate = appliedDate ?? DateTime.UtcNow
        };
        db.JobApplications.Add(app);
        db.SaveChanges();
        return app;
    }
}
