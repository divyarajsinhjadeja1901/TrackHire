using Microsoft.EntityFrameworkCore;
using TrackHire.API.Data;
using TrackHire.API.Models;

namespace TrackHire.API.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _context;
    public  ApplicationRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<JobApplication>> GetAllByUserIdAsync(int userId, string? statusFilter)
    {
        var query = _context.JobApplications
            .Where(j => j.UserId == userId);

        if (!string.IsNullOrEmpty(statusFilter))
            query = query.Where(j => j.Status == statusFilter);
        return await query.OrderByDescending(j => j.AppliedDate).ToListAsync();
    }
    public async Task<JobApplication?> GetByIdAsync(int id, int userId)
    {
        return await _context.JobApplications
            .Include(j => j.Notes)
            .Include(j => j.StatusHistories)
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);
    }
    public async Task<JobApplication> CreateAsync(JobApplication application)
    {
        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }
    public async Task<JobApplication?> UpdateStatusAsync(int id, int userId, string newStatus)
    {
        var app = await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        if (app == null) return null;
        var history = new StatusHistory
        {
            JobApplicationId = app.Id,
            OldStatus = app.Status,
            NewStatus = newStatus,
            ChangedAt = DateTime.UtcNow
        };
        _context.StatusHistories.Add(history);
        app.Status = newStatus;
        app.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return app;
    }
    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var app = await _context.JobApplications
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        if (app == null) return false;

        _context.JobApplications.Remove(app);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task AddNoteAsync(Note note)
    {
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
    }
}
