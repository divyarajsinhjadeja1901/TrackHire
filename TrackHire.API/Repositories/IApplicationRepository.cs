using TrackHire.API.Models;

namespace TrackHire.API.Repositories;

public interface IApplicationRepository
{
    Task<IEnumerable<JobApplication>> GetAllByUserIdAsync(int userId, string? statusFilter);
    Task<JobApplication?> GetByIdAsync(int id, int userId);
    Task<JobApplication> CreateAsync(JobApplication application);
    Task<JobApplication?> UpdateStatusAsync(int id, int userId, string newStatus);
    Task<bool> DeleteAsync(int id, int userId);
    Task AddNoteAsync(Note note);
}
