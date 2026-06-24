namespace HRMS.API.Models.Entities;

public class SalaryComponent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsRecurring { get; set; } = true;

    public int PayrollStructureId { get; set; }
    public PayrollStructure PayrollStructure { get; set; } = null!;
}
