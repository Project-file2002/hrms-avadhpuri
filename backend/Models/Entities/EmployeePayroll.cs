namespace HRMS.API.Models.Entities;

public class EmployeePayroll
{
    public int Id { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int PayrollStructureId { get; set; }
    public PayrollStructure PayrollStructure { get; set; } = null!;
}
