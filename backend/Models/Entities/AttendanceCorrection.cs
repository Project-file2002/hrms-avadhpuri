namespace HRMS.API.Models.Entities;

public class AttendanceCorrection
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public DateTime? CorrectedCheckIn { get; set; }
    public DateTime? CorrectedCheckOut { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int? ReviewedBy { get; set; }
    public string? ReviewNotes { get; set; }
}
