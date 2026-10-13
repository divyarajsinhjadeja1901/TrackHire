using Microsoft.EntityFrameworkCore;
using TrackHire.API.DTOs;
using TrackHire.API.Services;
using Xunit;

namespace TrackHire.Tests;

public class AuthServiceTests
{
    private static RegisterDto NewUser(string email = "a@example.com", string password = "Secret#123") =>
        new() { Name = "Alice", Email = email, Password = password };

    [Fact]
    public async Task Register_ReturnsToken_ForNewEmail()
    {
        using var db = TestHelpers.CreateDb();
        var service = new AuthService(db, TestHelpers.CreateJwtHelper());

        var token = await service.RegisterAsync(NewUser());

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task Register_StoresBCryptHash_NotThePlainPassword()
    {
        using var db = TestHelpers.CreateDb();
        var service = new AuthService(db, TestHelpers.CreateJwtHelper());

        await service.RegisterAsync(NewUser(password: "Secret#123"));
        var stored = await db.Users.SingleAsync();

        Assert.NotEqual("Secret#123", stored.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Secret#123", stored.PasswordHash));
    }

    [Fact]
    public async Task Register_ReturnsNull_AndCreatesNoUser_ForDuplicateEmail()
    {
        using var db = TestHelpers.CreateDb();
        var service = new AuthService(db, TestHelpers.CreateJwtHelper());
        await service.RegisterAsync(NewUser());

        var second = await service.RegisterAsync(NewUser());

        Assert.Null(second);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task Login_ReturnsToken_ForCorrectCredentials()
    {
        using var db = TestHelpers.CreateDb();
        var service = new AuthService(db, TestHelpers.CreateJwtHelper());
        await service.RegisterAsync(NewUser());

        var token = await service.LoginAsync(new LoginDto { Email = "a@example.com", Password = "Secret#123" });

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_ReturnsNull_ForWrongPassword()
    {
        using var db = TestHelpers.CreateDb();
        var service = new AuthService(db, TestHelpers.CreateJwtHelper());
        await service.RegisterAsync(NewUser());

        var token = await service.LoginAsync(new LoginDto { Email = "a@example.com", Password = "wrong" });

        Assert.Null(token);
    }

    [Fact]
    public async Task Login_ReturnsNull_ForUnknownEmail()
    {
        using var db = TestHelpers.CreateDb();
        var service = new AuthService(db, TestHelpers.CreateJwtHelper());

        var token = await service.LoginAsync(new LoginDto { Email = "nobody@example.com", Password = "x" });

        Assert.Null(token);
    }
}
