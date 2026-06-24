namespace HRMS.API.Models.Entities;

public class PayrollStructure
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<SalaryComponent> Components { get; set; } = new List<SalaryComponent>();
}
