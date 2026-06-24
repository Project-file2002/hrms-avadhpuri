namespace HRMS.API.Models.Entities;

public class InterviewSchedule
{
    public int Id { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? InterviewerName { get; set; }
    public string? InterviewType { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? Feedback { get; set; }
    public int? Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CandidateId { get; set; }
    public CandidateProfile Candidate { get; set; } = null!;
}
