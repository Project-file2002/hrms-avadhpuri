using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRMS.API.Models.DTOs.Payroll;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager,Manager,PayrollStaff")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet("structures")]
    public async Task<IActionResult> GetStructures()
    {
        var structures = await _payrollService.GetStructuresAsync();
        return Ok(structures);
    }

    [HttpGet("structures/{id}")]
    public async Task<IActionResult> GetStructureById(int id)
    {
        var structure = await _payrollService.GetStructureByIdAsync(id);
        if (structure == null) return NotFound();
        return Ok(structure);
    }

    [HttpPost("structures")]
    public async Task<IActionResult> CreateStructure([FromBody] CreatePayrollStructure request)
    {
        var structure = await _payrollService.CreateStructureAsync(request);
        return CreatedAtAction(nameof(GetStructureById), new { id = structure.Id }, structure);
    }

    [HttpPost("components")]
    public async Task<IActionResult> AddComponent([FromBody] CreateSalaryComponent request)
    {
        var component = await _payrollService.AddComponentAsync(request);
        return Ok(component);
    }
}
