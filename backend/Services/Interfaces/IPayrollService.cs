using HRMS.API.Models.DTOs.Payroll;

namespace HRMS.API.Services.Interfaces;

public interface IPayrollService
{
    Task<IEnumerable<PayrollStructureDto>> GetStructuresAsync();
    Task<PayrollStructureDto?> GetStructureByIdAsync(int id);
    Task<PayrollStructureDto> CreateStructureAsync(CreatePayrollStructure request);
    Task<SalaryComponentDto> AddComponentAsync(CreateSalaryComponent request);
}
