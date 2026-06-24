using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRMS.API.Models.DTOs.Department;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [Authorize(Roles = "Administrator,HRManager,Manager,Employee")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _departmentService.GetAllAsync();
        return Ok(departments);
    }

    [Authorize(Roles = "Administrator,HRManager,Manager,Employee")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        if (department == null) return NotFound();
        return Ok(department);
    }

    [Authorize(Roles = "Administrator,HRManager,Manager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
    {
        var department = await _departmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    [Authorize(Roles = "Administrator,HRManager,Manager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentRequest request)
    {
        var department = await _departmentService.UpdateAsync(id, request);
        if (department == null) return NotFound();
        return Ok(department);
    }

    [Authorize(Roles = "Administrator,HRManager,Manager,Employee")]
    [HttpGet("org-chart")]
    public async Task<IActionResult> GetOrgChart()
    {
        var chart = await _departmentService.GetOrgChartAsync();
        return Ok(chart);
    }

    [Authorize(Roles = "Administrator,HRManager,Manager")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _departmentService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
