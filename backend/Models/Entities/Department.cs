namespace HRMS.API.Models.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public int? HeadId { get; set; }
    public Employee? Head { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
