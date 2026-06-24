namespace HRMS.API.Models.Entities;

public class BackgroundCheck
{
    public int Id { get; set; }
    public string Status { get; set; } = "Pending";
    public string? VendorName { get; set; }
    public string? ReportPath { get; set; }
    public string? Notes { get; set; }
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public int CandidateId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
}
