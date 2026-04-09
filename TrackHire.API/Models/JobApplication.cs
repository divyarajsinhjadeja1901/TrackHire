namespace TrackHire.API.Models
{
    public class JobApplication
    {
        public int Id { get; set; }
        public int UserId { get; set; } //foreign key
        public string CompanyName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string JobUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Applied"; // Applied, Interview, Offer, Rejected, Withdrawn
        public string Location { get; set; } = string.Empty;
        public decimal? ExpectedSalary { get; set; }
        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
        public DateTime? InterviewDate { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //Navigation Properties
        public User User { get; set; } = null!;
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
    }
}
