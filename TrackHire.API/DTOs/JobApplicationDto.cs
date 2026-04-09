namespace TrackHire.API.DTOs;

public class JobApplicationDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string JobUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal? ExpectedSalary { get; set; }
    public DateTime AppliedDate { get; set; }
    public DateTime? InterviewDate { get; set; }
}
