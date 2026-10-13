using Moq;
using TrackHire.API.DTOs;
using TrackHire.API.Models;
using TrackHire.API.Repositories;
using TrackHire.API.Services;
using Xunit;

namespace TrackHire.Tests;

public class ApplicationServiceTests
{
    private readonly Mock<IApplicationRepository> _repo = new();
    private ApplicationService CreateService() => new(_repo.Object);

    [Fact]
    public async Task CreateAsync_AssignsUserId_AndReturnsMappedDto()
    {
        JobApplication? saved = null;
        _repo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>()))
             .Callback<JobApplication>(a => saved = a)
             .ReturnsAsync((JobApplication a) => { a.Id = 7; return a; });

        var dto = await CreateService().CreateAsync(42, new CreateApplicationDto
        {
            CompanyName = "Honeywell",
            JobTitle = "Software Engineer I",
            Location = "Pune"
        });

        Assert.NotNull(saved);
        Assert.Equal(42, saved!.UserId);
        Assert.Equal(7, dto.Id);
        Assert.Equal("Honeywell", dto.CompanyName);
        Assert.Equal("Applied", dto.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenApplicationNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync((JobApplication?)null);

        var result = await CreateService().GetByIdAsync(1, 10);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_MapsEntityFieldsToDto()
    {
        _repo.Setup(r => r.GetByIdAsync(3, 10)).ReturnsAsync(new JobApplication
        {
            Id = 3, UserId = 10, CompanyName = "Acme", JobTitle = "Dev",
            Status = "Interview", Location = "Pune", ExpectedSalary = 900000m
        });

        var result = await CreateService().GetByIdAsync(3, 10);

        Assert.NotNull(result);
        Assert.Equal("Acme", result!.CompanyName);
        Assert.Equal("Interview", result.Status);
        Assert.Equal(900000m, result.ExpectedSalary);
    }

    [Fact]
    public async Task GetAllAsync_PassesUserIdAndStatusFilterToRepository()
    {
        IEnumerable<JobApplication> empty = new List<JobApplication>();
        _repo.Setup(r => r.GetAllByUserIdAsync(5, "Interview")).ReturnsAsync(empty);

        await CreateService().GetAllAsync(5, "Interview");

        _repo.Verify(r => r.GetAllByUserIdAsync(5, "Interview"), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOneDtoPerApplication()
    {
        IEnumerable<JobApplication> apps = new List<JobApplication>
        {
            new() { Id = 1, CompanyName = "A" },
            new() { Id = 2, CompanyName = "B" }
        };
        _repo.Setup(r => r.GetAllByUserIdAsync(5, null)).ReturnsAsync(apps);

        var result = (await CreateService().GetAllAsync(5, null)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(new[] { "A", "B" }, result.Select(r => r.CompanyName));
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsTrue_WhenRepositoryUpdatesApplication()
    {
        _repo.Setup(r => r.UpdateStatusAsync(1, 10, "Offer"))
             .ReturnsAsync(new JobApplication { Id = 1, Status = "Offer" });

        Assert.True(await CreateService().UpdateStatusAsync(1, 10, "Offer"));
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenApplicationNotFound()
    {
        _repo.Setup(r => r.UpdateStatusAsync(1, 10, "Offer")).ReturnsAsync((JobApplication?)null);

        Assert.False(await CreateService().UpdateStatusAsync(1, 10, "Offer"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAsync_ReturnsWhatTheRepositoryReturns(bool repoResult)
    {
        _repo.Setup(r => r.DeleteAsync(1, 10)).ReturnsAsync(repoResult);

        Assert.Equal(repoResult, await CreateService().DeleteAsync(1, 10));
    }

    [Fact]
    public async Task AddNoteAsync_DoesNotSaveNote_WhenApplicationBelongsToSomeoneElse()
    {
        _repo.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync((JobApplication?)null);

        await CreateService().AddNoteAsync(1, 10, "hello");

        _repo.Verify(r => r.AddNoteAsync(It.IsAny<Note>()), Times.Never);
    }

    [Fact]
    public async Task AddNoteAsync_SavesNote_WhenApplicationIsOwnedByUser()
    {
        _repo.Setup(r => r.GetByIdAsync(1, 10)).ReturnsAsync(new JobApplication { Id = 1, UserId = 10 });
        Note? saved = null;
        _repo.Setup(r => r.AddNoteAsync(It.IsAny<Note>()))
             .Callback<Note>(n => saved = n)
             .Returns(Task.CompletedTask);

        await CreateService().AddNoteAsync(1, 10, "Follow up on Friday");

        Assert.NotNull(saved);
        Assert.Equal(1, saved!.JobApplicationId);
        Assert.Equal("Follow up on Friday", saved.Content);
    }
}
