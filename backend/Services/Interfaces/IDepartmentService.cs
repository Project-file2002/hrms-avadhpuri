using HRMS.API.Models.DTOs.Department;

namespace HRMS.API.Services.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto?> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request);
    Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentRequest request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<DepartmentOrgNode>> GetOrgChartAsync();
}

public class DepartmentOrgNode
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? HeadName { get; set; }
    public int EmployeeCount { get; set; }
    public List<OrgEmployee> Employees { get; set; } = new();
}

public class OrgEmployee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Position { get; set; }
    public bool IsHead { get; set; }
}
