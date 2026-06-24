namespace HRMS.API.Models.DTOs.Payroll;

public class PayrollStructureDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<SalaryComponentDto> Components { get; set; } = new();
}

public class SalaryComponentDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CreatePayrollStructure
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateSalaryComponent
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PayrollStructureId { get; set; }
}
