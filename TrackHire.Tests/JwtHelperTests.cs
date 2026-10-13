using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TrackHire.API.Models;
using Xunit;

namespace TrackHire.Tests;

public class JwtHelperTests
{
    private static readonly User TestUser = new() { Id = 5, Name = "Divya", Email = "divya@example.com" };

    [Fact]
    public void GenerateToken_ContainsUserIdEmailAndName()
    {
        var token = new JwtSecurityTokenHandler()
            .ReadJwtToken(TestHelpers.CreateJwtHelper().GenerateToken(TestUser));

        Assert.Contains(token.Claims, c => c.Value == "5");
        Assert.Contains(token.Claims, c => c.Value == "divya@example.com");
        Assert.Contains(token.Claims, c => c.Value == "Divya");
    }

    [Fact]
    public void GenerateToken_SetsIssuerAudienceAndSevenDayExpiry()
    {
        var token = new JwtSecurityTokenHandler()
            .ReadJwtToken(TestHelpers.CreateJwtHelper().GenerateToken(TestUser));

        Assert.Equal("TrackHireAPI", token.Issuer);
        Assert.Contains("TrackHireClient", token.Audiences);
        Assert.InRange(token.ValidTo, DateTime.UtcNow.AddDays(6), DateTime.UtcNow.AddDays(8));
    }

    [Fact]
    public void GenerateToken_IsAcceptedWhenValidatedWithTheSameKey()
    {
        var jwt = TestHelpers.CreateJwtHelper().GenerateToken(TestUser);

        var principal = new JwtSecurityTokenHandler().ValidateToken(jwt, ValidationParameters(TestHelpers.JwtKey), out var validated);

        Assert.NotNull(principal);
        Assert.NotNull(validated);
    }

    [Fact]
    public void GenerateToken_IsRejectedWhenValidatedWithADifferentKey()
    {
        var jwt = TestHelpers.CreateJwtHelper().GenerateToken(TestUser);

        Assert.ThrowsAny<SecurityTokenException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(
                jwt, ValidationParameters("a-completely-different-key-of-32-plus-chars!!"), out _));
    }

    private static TokenValidationParameters ValidationParameters(string key) => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "TrackHireAPI",
        ValidAudience = "TrackHireClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
}
