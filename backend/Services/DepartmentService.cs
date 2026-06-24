using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Department;
using HRMS.API.Models.Entities;
using HRMS.API.Models.Mapping;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Services;

public class DepartmentService : IDepartmentService
{
    private readonly HRMSDbContext _context;

    public DepartmentService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
    {
        var departments = await _context.Departments
            .Include(d => d.Head)
            .Include(d => d.Employees)
            .Where(d => !d.IsDeleted)
            .ToListAsync();

        return departments.Select(d => d.ToDto());
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Head)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        return department?.ToDto();
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request)
    {
        var department = new Department
        {
            Name = request.Name,
            Description = request.Description,
            HeadId = request.HeadId
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return department.ToDto();
    }

    public async Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentRequest request)
    {
        var department = await _context.Departments
            .Include(d => d.Head)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (department == null) return null;

        if (request.Name != null) department.Name = request.Name;
        if (request.Description != null) department.Description = request.Description;
        if (request.HeadId.HasValue) department.HeadId = request.HeadId;

        await _context.SaveChangesAsync();
        return department.ToDto();
    }

    public async Task<IEnumerable<DepartmentOrgNode>> GetOrgChartAsync()
    {
        var departments = await _context.Departments
            .Include(d => d.Head)
            .Include(d => d.Employees.Where(e => !e.IsDeleted))
            .Where(d => !d.IsDeleted)
            .ToListAsync();

        return departments.Select(d => new DepartmentOrgNode
        {
            Id = d.Id,
            Name = d.Name,
            HeadName = d.Head != null ? $"{d.Head.FirstName} {d.Head.LastName}" : null,
            EmployeeCount = d.Employees.Count,
            Employees = d.Employees.Select(e => new OrgEmployee
            {
                Id = e.Id,
                Name = $"{e.FirstName} {e.LastName}",
                Position = e.Position,
                IsHead = e.Id == d.HeadId
            }).OrderByDescending(e => e.IsHead).ToList()
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null) return false;

        department.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
