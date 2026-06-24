namespace HRMS.API.Models.Entities;

public class TransferRequest
{
    public int Id { get; set; }
    public string CurrentPosition { get; set; } = string.Empty;
    public string ProposedPosition { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string Status { get; set; } = "PendingManagerApproval";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? ManagerNotes { get; set; }
    public string? HrNotes { get; set; }
    public string? DeptNotes { get; set; }
    public string? ItNotes { get; set; }
    public string? PayrollNotes { get; set; }
    public string? EmployeeNotes { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int RequestedById { get; set; }
    public Employee RequestedBy { get; set; } = null!;

    public int CurrentDepartmentId { get; set; }
    public Department CurrentDepartment { get; set; } = null!;

    public int ProposedDepartmentId { get; set; }
    public Department ProposedDepartment { get; set; } = null!;

    public int? ApprovedByManagerId { get; set; }
    public Employee? ApprovedByManager { get; set; }

    public int? ApprovedByHrId { get; set; }
    public Employee? ApprovedByHr { get; set; }

    public int? ApprovedByDeptId { get; set; }
    public Employee? ApprovedByDept { get; set; }

    public int? ApprovedByItId { get; set; }
    public Employee? ApprovedByIt { get; set; }

    public int? ApprovedByPayrollId { get; set; }
    public Employee? ApprovedByPayroll { get; set; }

    public bool EmployeeAccepted { get; set; }
}
