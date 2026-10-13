using Microsoft.EntityFrameworkCore;
using TrackHire.API.Repositories;
using Xunit;

namespace TrackHire.Tests;

// These tests verify per-user data isolation, the core security rule of TrackHire.
public class ApplicationRepositoryTests
{
    [Fact]
    public async Task GetAllByUserId_ReturnsOnlyThatUsersApplications()
    {
        using var db = TestHelpers.CreateDb();
        TestHelpers.SeedUser(db, 1, "u1@example.com");
        TestHelpers.SeedUser(db, 2, "u2@example.com");
        TestHelpers.SeedApplication(db, 1, "Mine");
        TestHelpers.SeedApplication(db, 2, "Theirs");

        var result = (await new ApplicationRepository(db).GetAllByUserIdAsync(1, null)).ToList();

        Assert.Single(result);
        Assert.Equal("Mine", result[0].CompanyName);
    }

    [Fact]
    public async Task GetAllByUserId_FiltersByStatus()
    {
        using var db = TestHelpers.CreateDb();
        TestHelpers.SeedUser(db, 1, "u1@example.com");
        TestHelpers.SeedApplication(db, 1, "A", "Applied");
        TestHelpers.SeedApplication(db, 1, "B", "Interview");

        var result = (await new ApplicationRepository(db).GetAllByUserIdAsync(1, "Interview")).ToList();

        Assert.Single(result);
        Assert.Equal("B", result[0].CompanyName);
    }

    [Fact]
    public async Task GetAllByUserId_ReturnsNewestApplicationFirst()
    {
        using var db = TestHelpers.CreateDb();
        TestHelpers.SeedUser(db, 1, "u1@example.com");
        TestHelpers.SeedApplication(db, 1, "Old", appliedDate: new DateTime(2026, 1, 1));
        TestHelpers.SeedApplication(db, 1, "New", appliedDate: new DateTime(2026, 6, 1));

        var result = (await new ApplicationRepository(db).GetAllByUserIdAsync(1, null)).ToList();

        Assert.Equal(new[] { "New", "Old" }, result.Select(a => a.CompanyName));
    }

    [Fact]
    public async Task GetById_ReturnsNull_ForAnotherUsersApplication()
    {
        using var db = TestHelpers.CreateDb();
        TestHelpers.SeedUser(db, 1, "u1@example.com");
        TestHelpers.SeedUser(db, 2, "u2@example.com");
        var theirs = TestHelpers.SeedApplication(db, 2, "Theirs");

        var result = await new ApplicationRepository(db).GetByIdAsync(theirs.Id, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStatus_ChangesStatus_AndWritesStatusHistoryRow()
    {
        using var db = TestHelpers.CreateDb();
        TestHelpers.SeedUser(db, 1, "u1@example.com");
        var app = TestHelpers.SeedApplication(db, 1, status: "Applied");

        var updated = await new ApplicationRepository(db).UpdateStatusAsync(app.Id, 1, "Interview");

        Assert.NotNull(updated);
        Assert.Equal("Interview", updated!.Status);
        var history = await db.StatusHistories.SingleAsync();
        Assert.Equal("Applied", history.OldStatus);
        Assert.Equal("Interview", history.NewStatus);
        Assert.Equal(app.Id, history.JobApplicationId);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsNull_AndWritesNoHistory_ForAnotherUsersApplication()
    {
        using var db = TestHelpers.CreateDb();
        TestHelpers.SeedUser(db, 1, "u1@example.com");
        TestHelpers.SeedUser(db, 2, "u2@example.com");
        var theirs = TestHelpers.SeedApplication(db, 2, status: "Applied");

        var result = await new ApplicationRepository(db).UpdateStatusAsync(theirs.Id, 1, "Offer");

        Assert.Null(result);
        Assert.Empty(db.StatusHistories);
        Assert.Equal("Applied", (await db.JobApplications.SingleAsync()).Status);
    }

    [Fact]
    public async Task Delete_RemovesOwnApplication()
    {
        using var db = TestHelpers.CreateDb();
        TestHelpers.SeedUser(db, 1, "u1@example.com");
        var mine = TestHelpers.SeedApplication(db, 1);

        var deleted = await new ApplicationRepository(db).DeleteAsync(mine.Id, 1);

        Assert.True(deleted);
        Assert.Empty(db.JobApplications);
    }

    [Fact]
    public async Task Delete_ReturnsFalse_AndKeepsRecord_ForAnotherUsersApplication()
    {
        using var db = TestHelpers.CreateDb();
        TestHelpers.SeedUser(db, 1, "u1@example.com");
        TestHelpers.SeedUser(db, 2, "u2@example.com");
        var theirs = TestHelpers.SeedApplication(db, 2);

        var deleted = await new ApplicationRepository(db).DeleteAsync(theirs.Id, 1);

        Assert.False(deleted);
        Assert.Single(db.JobApplications);
    }
}
