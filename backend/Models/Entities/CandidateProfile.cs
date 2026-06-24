namespace HRMS.API.Models.Entities;

public class CandidateProfile
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ResumePath { get; set; }
    public string? ResumeText { get; set; }
    public string? ScreeningSummary { get; set; }
    public string Status { get; set; } = "New";
    public string? Source { get; set; }
    public decimal? MatchScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }
    public int? JobRequisitionId { get; set; }
    public JobRequisition? JobRequisition { get; set; }
    public ICollection<InterviewSchedule> Interviews { get; set; } = new List<InterviewSchedule>();
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    public ICollection<BackgroundCheck> BackgroundChecks { get; set; } = new List<BackgroundCheck>();
    public ICollection<OnboardingTask> OnboardingTasks { get; set; } = new List<OnboardingTask>();
}
