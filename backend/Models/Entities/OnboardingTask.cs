namespace HRMS.API.Models.Entities;

public class OnboardingTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "Document";
    public string? AssignedTo { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int CandidateId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
}
