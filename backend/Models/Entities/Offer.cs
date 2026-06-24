namespace HRMS.API.Models.Entities;

public class Offer
{
    public int Id { get; set; }
    public decimal Salary { get; set; }
    public string? Currency { get; set; } = "INR";
    public string? Benefits { get; set; }
    public DateTime? StartDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string? OfferLetterPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    public int CandidateId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;

    public int? ApprovedById { get; set; }
    public Employee? ApprovedBy { get; set; }
}
