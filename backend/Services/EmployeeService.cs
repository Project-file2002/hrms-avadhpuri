using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Employee;
using HRMS.API.Models.Entities;
using HRMS.API.Models.Mapping;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Services;

public class EmployeeService : IEmployeeService
{
    private readonly HRMSDbContext _context;

    public EmployeeService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Where(e => !e.IsDeleted)
            .ToListAsync();

        return employees.Select(e => e.ToDto());
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        return employee?.ToDto();
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request)
    {
        var employee = new Employee
        {
            EmployeeCode = request.EmployeeCode,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Position = request.Position,
            DateOfBirth = request.DateOfBirth,
            DateOfJoining = request.DateOfJoining,
            Gender = request.Gender,
            Address = request.Address,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            DepartmentId = request.DepartmentId,
            ManagerId = request.ManagerId
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        await _context.Entry(employee).Reference(e => e.Department).LoadAsync();
        await _context.Entry(employee).Reference(e => e.Manager).LoadAsync();

        return employee.ToDto();
    }

    public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeRequest request)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        if (employee == null) return null;

        if (request.FirstName != null) employee.FirstName = request.FirstName;
        if (request.LastName != null) employee.LastName = request.LastName;
        if (request.Phone != null) employee.Phone = request.Phone;
        if (request.Position != null) employee.Position = request.Position;
        if (request.DateOfBirth.HasValue) employee.DateOfBirth = request.DateOfBirth;
        if (request.DateOfJoining.HasValue) employee.DateOfJoining = request.DateOfJoining;
        if (request.Status != null) employee.Status = request.Status;
        if (request.Gender != null) employee.Gender = request.Gender;
        if (request.Address != null) employee.Address = request.Address;
        if (request.EmergencyContactName != null) employee.EmergencyContactName = request.EmergencyContactName;
        if (request.EmergencyContactPhone != null) employee.EmergencyContactPhone = request.EmergencyContactPhone;
        if (request.DepartmentId.HasValue) employee.DepartmentId = request.DepartmentId;
        if (request.ManagerId.HasValue) employee.ManagerId = request.ManagerId;

        employee.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return employee.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        employee.IsDeleted = true;
        employee.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
