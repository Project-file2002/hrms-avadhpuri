namespace HRMS.API.Models.Entities;

public class Announcement
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Scope { get; set; } = "Company";
    public string Priority { get; set; } = "Normal";
    public bool AcknowledgementRequired { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public int CreatedById { get; set; }
    public Employee CreatedBy { get; set; } = null!;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<AnnouncementRead> Reads { get; set; } = new List<AnnouncementRead>();
}

public class AnnouncementRead
{
    public int Id { get; set; }
    public int AnnouncementId { get; set; }
    public Announcement Announcement { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
}
