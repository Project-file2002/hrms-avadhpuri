namespace HRMS.API.Models.Entities;

public class ComplianceRecord
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Statutory";
    public string Regulation { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? AssignedToId { get; set; }
    public Employee? AssignedTo { get; set; }

    public int? CompletedById { get; set; }
    public Employee? CompletedBy { get; set; }
}

public class DataPrivacyLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string DataCategory { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string ConsentStatus { get; set; } = "Granted";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }
}
