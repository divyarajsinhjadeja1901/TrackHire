using TrackHire.API.DTOs;
using TrackHire.API.Models;
using TrackHire.API.Repositories;
namespace TrackHire.API.Services;
public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _repo;
    public ApplicationService(IApplicationRepository repo)
    {
        _repo = repo;
    }
    public async Task<IEnumerable<JobApplicationDto>> GetAllAsync(int userId, string? statusFilter)
    {
        var apps = await _repo.GetAllByUserIdAsync(userId, statusFilter);
        return apps.Select(a => new JobApplicationDto
        {
            Id = a.Id,
            CompanyName = a.CompanyName,
            JobTitle = a.JobTitle,
            JobUrl = a.JobUrl,
            Status = a.Status,
            Location = a.Location,
            ExpectedSalary = a.ExpectedSalary,
            AppliedDate = a.AppliedDate,
            InterviewDate = a.InterviewDate
        });
    }
    public async Task<JobApplicationDto?> GetByIdAsync(int id, int userId)
    {
        var a = await _repo.GetByIdAsync(id, userId);
        if (a == null) return null;

        return new JobApplicationDto
        {
            Id = a.Id,
            CompanyName = a.CompanyName,
            JobTitle = a.JobTitle,
            JobUrl = a.JobUrl,
            Status = a.Status,
            Location = a.Location,
            ExpectedSalary = a.ExpectedSalary,
            AppliedDate = a.AppliedDate,
            InterviewDate = a.InterviewDate
        };
    }
    public async Task<JobApplicationDto> CreateAsync(int userId, CreateApplicationDto dto)
    {
        var app = new JobApplication
        {
            UserId = userId,
            CompanyName = dto.CompanyName,
            JobTitle = dto.JobTitle,
            JobUrl = dto.JobUrl,
            Location = dto.Location,
            ExpectedSalary = dto.ExpectedSalary,
            InterviewDate = dto.InterviewDate
        };
        var created = await _repo.CreateAsync(app);
        return new JobApplicationDto
        {
            Id = created.Id,
            CompanyName = created.CompanyName,
            JobTitle = created.JobTitle,
            Status = created.Status,
            Location = created.Location,
            AppliedDate = created.AppliedDate
        };
    }
    public async Task<bool> UpdateStatusAsync(int id, int userId, string newStatus)
    {
        var result = await _repo.UpdateStatusAsync(id, userId, newStatus);
        return result != null;
    }
    public async Task<bool> DeleteAsync(int id, int userId) =>
        await _repo.DeleteAsync(id, userId);
    public async Task AddNoteAsync(int applicationId, int userId, string content)
    {
        var app = await _repo.GetByIdAsync(applicationId, userId);
        if (app == null) return;

        await _repo.AddNoteAsync(new Note
        {
            JobApplicationId = applicationId,
            Content = content
        });
    }
}