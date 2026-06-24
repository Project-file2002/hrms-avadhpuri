using HRMS.API.Models.DTOs.Employee;

namespace HRMS.API.Services.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync();
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request);
    Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeRequest request);
    Task<bool> DeleteAsync(int id);
}
