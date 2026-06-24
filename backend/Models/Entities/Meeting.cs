namespace HRMS.API.Models.Entities;

public class Meeting
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Agenda { get; set; }
    public string MeetingType { get; set; } = "Team";
    public string Status { get; set; } = "Scheduled";
    public string Priority { get; set; } = "Normal";
    public string Timezone { get; set; } = "Asia/Kolkata";
    public string? Recurrence { get; set; }
    public string? Location { get; set; }
    public string? OnlineLink { get; set; }
    public string? AiSummary { get; set; }
    public string? AiMomTemplate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int OrganizerId { get; set; }
    public Employee Organizer { get; set; } = null!;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
}

public class MeetingParticipant
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public string ResponseStatus { get; set; } = "Pending";
    public bool IsOptional { get; set; }
}
