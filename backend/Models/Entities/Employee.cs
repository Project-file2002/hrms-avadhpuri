namespace HRMS.API.Models.Entities;

public class Employee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public string Status { get; set; } = "Active";
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public User? User { get; set; }
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
}
