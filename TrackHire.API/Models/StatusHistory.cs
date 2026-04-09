namespace TrackHire.API.Models;

public class StatusHistory
{
    public int Id { get; set; }
    public int JobApplicationId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public JobApplication JobApplication { get; set; } = null!;
}
