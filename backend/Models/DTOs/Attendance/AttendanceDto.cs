namespace HRMS.API.Models.DTOs.Attendance;

public class AttendanceRecordDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
}

public class CreateAttendanceRecord
{
    public DateTime Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
}

public class AttendanceCorrectionDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public DateTime? CorrectedCheckIn { get; set; }
    public DateTime? CorrectedCheckOut { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class CreateAttendanceCorrection
{
    public DateTime Date { get; set; }
    public DateTime? CorrectedCheckIn { get; set; }
    public DateTime? CorrectedCheckOut { get; set; }
    public string Reason { get; set; } = string.Empty;
}
