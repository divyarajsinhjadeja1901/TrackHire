using TrackHire.API.DTOs;
namespace TrackHire.API.Services;
public interface IApplicationService
{
    Task<IEnumerable<JobApplicationDto>> GetAllAsync(int userId, string? statusFilter);
    Task<JobApplicationDto?> GetByIdAsync(int id, int userId);
    Task<JobApplicationDto> CreateAsync(int userId, CreateApplicationDto dto);
    Task<bool> UpdateStatusAsync(int id, int userId, string newStatus);
    Task<bool> DeleteAsync(int id, int userId);
    Task AddNoteAsync(int applicationId, int userId, string content);
}