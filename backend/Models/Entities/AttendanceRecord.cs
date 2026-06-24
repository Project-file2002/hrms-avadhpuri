namespace HRMS.API.Models.Entities;

public class AttendanceRecord
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string Status { get; set; } = "Present";

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}
