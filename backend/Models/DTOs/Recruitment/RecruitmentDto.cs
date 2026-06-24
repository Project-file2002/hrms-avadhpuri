namespace HRMS.API.Models.DTOs.Recruitment;

public class CandidateDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? MatchScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? JobTitle { get; set; }
}

public class CreateCandidate
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Source { get; set; }
    public int? JobRequisitionId { get; set; }
}

public class JobRequisitionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CandidateCount { get; set; }
}

public class CreateJobRequisition
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public int? DepartmentId { get; set; }
}

public class InterviewDto
{
    public int Id { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? InterviewerName { get; set; }
    public string? InterviewType { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Feedback { get; set; }
    public int? Rating { get; set; }
}

public class CreateInterview
{
    public DateTime ScheduledDate { get; set; }
    public string? InterviewerName { get; set; }
    public string? InterviewType { get; set; }
    public int CandidateId { get; set; }
}
