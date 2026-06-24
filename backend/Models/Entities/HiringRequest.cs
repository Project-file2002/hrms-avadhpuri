namespace HRMS.API.Models.Entities;

public class HiringRequest
{
    public int Id { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Justification { get; set; }
    public int Headcount { get; set; } = 1;
    public decimal? BudgetRangeLow { get; set; }
    public decimal? BudgetRangeHigh { get; set; }
    public string EmploymentType { get; set; } = "FullTime";
    public string Status { get; set; } = "PendingDeptApproval";
    public string? DeptApprovalNotes { get; set; }
    public string? HrApprovalNotes { get; set; }
    public string? BudgetApprovalNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int RequestedById { get; set; }
    public Employee RequestedBy { get; set; } = null!;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? JobRequisitionId { get; set; }
    public JobRequisition? JobRequisition { get; set; }
}
