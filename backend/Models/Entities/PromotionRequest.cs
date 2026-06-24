namespace HRMS.API.Models.Entities;

public class PromotionRequest
{
    public int Id { get; set; }
    public string CurrentPosition { get; set; } = string.Empty;
    public decimal CurrentSalary { get; set; }
    public string ProposedPosition { get; set; } = string.Empty;
    public decimal ProposedSalary { get; set; }
    public string? Justification { get; set; }
    public string Status { get; set; } = "PendingManagerApproval";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? ManagerNotes { get; set; }
    public string? HrbpNotes { get; set; }
    public string? DeptHeadNotes { get; set; }
    public string? CeoNotes { get; set; }
    public bool PayrollUpdated { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int RequestedById { get; set; }
    public Employee RequestedBy { get; set; } = null!;

    public int? ApprovedByManagerId { get; set; }
    public Employee? ApprovedByManager { get; set; }

    public int? ApprovedByHrbpId { get; set; }
    public Employee? ApprovedByHrbp { get; set; }

    public int? ApprovedByDeptHeadId { get; set; }
    public Employee? ApprovedByDeptHead { get; set; }

    public int? ApprovedByCeoId { get; set; }
    public Employee? ApprovedByCeo { get; set; }
}
