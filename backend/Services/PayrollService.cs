using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Payroll;
using HRMS.API.Models.Entities;
using HRMS.API.Models.Mapping;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Services;

public class PayrollService : IPayrollService
{
    private readonly HRMSDbContext _context;

    public PayrollService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PayrollStructureDto>> GetStructuresAsync()
    {
        var structures = await _context.PayrollStructures
            .Include(ps => ps.Components)
            .ToListAsync();

        return structures.Select(ps => ps.ToDto());
    }

    public async Task<PayrollStructureDto?> GetStructureByIdAsync(int id)
    {
        var structure = await _context.PayrollStructures
            .Include(ps => ps.Components)
            .FirstOrDefaultAsync(ps => ps.Id == id);

        return structure?.ToDto();
    }

    public async Task<PayrollStructureDto> CreateStructureAsync(CreatePayrollStructure request)
    {
        var structure = new PayrollStructure
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.PayrollStructures.Add(structure);
        await _context.SaveChangesAsync();

        return structure.ToDto();
    }

    public async Task<SalaryComponentDto> AddComponentAsync(CreateSalaryComponent request)
    {
        var component = new SalaryComponent
        {
            Name = request.Name,
            Type = request.Type,
            Amount = request.Amount,
            PayrollStructureId = request.PayrollStructureId
        };

        _context.SalaryComponents.Add(component);
        await _context.SaveChangesAsync();

        return component.ToDto();
    }
}
