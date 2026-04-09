namespace TrackHire.API.DTOs;

public class CreateApplicationDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string JobUrl { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal? ExpectedSalary { get; set; }
    public DateTime? InterviewDate { get; set; }
}
