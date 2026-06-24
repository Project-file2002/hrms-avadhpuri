namespace HRMS.API.Models.Entities;

public class LeaveBalance
{
    public int Id { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public int Year { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;
}
