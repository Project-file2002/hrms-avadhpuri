using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Employee;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    private readonly HRMSDbContext _context;

    public EmployeesController(IEmployeeService employeeService, HRMSDbContext context)
    {
        _employeeService = employeeService;
        _context = context;
    }

    [Authorize(Roles = "Administrator,HRManager,Manager,Employee")]
    [HttpGet("{id}/timeline")]
    public async Task<IActionResult> GetTimeline(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        var events = new List<object>();

        // Joining event
        if (employee.DateOfJoining.HasValue)
        {
            events.Add(new
            {
                date = employee.DateOfJoining.Value,
                type = "joining",
                title = "Joined the company",
                description = $"Started as {employee.Position ?? "Employee"}",
                color = "green"
            });
        }

        // Performance reviews
        var reviews = await _context.PerformanceReviews
            .Include(r => r.Cycle)
            .Where(r => r.EmployeeId == id)
            .OrderBy(r => r.EndDate)
            .ToListAsync();

        foreach (var review in reviews)
        {
            events.Add(new
            {
                date = review.EndDate,
                type = "review",
                title = $"Performance Review: {review.Title}",
                description = $"{review.Cycle.Name} — Score: {review.OverallScore?.ToString("F1") ?? "N/A"} — {review.Status}",
                color = review.Status == "Completed" ? "blue" : review.Status == "In Progress" ? "orange" : "gray"
            });
        }

        // Approved leaves
        var leaves = await _context.LeaveRequests
            .Include(l => l.LeaveType)
            .Where(l => l.EmployeeId == id && l.Status == "Approved")
            .OrderBy(l => l.StartDate)
            .ToListAsync();

        foreach (var leave in leaves)
        {
            events.Add(new
            {
                date = leave.StartDate,
                type = "leave",
                title = $"Leave: {leave.LeaveType.Name}",
                description = $"{leave.StartDate:dd MMM} → {leave.EndDate:dd MMM yyyy} — {leave.Reason}",
                color = "purple"
            });
        }

        return Ok(events.OrderBy(e => ((DateTime)((dynamic)e).date)));
    }

    [Authorize(Roles = "Administrator,HRManager,Manager,Employee")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _employeeService.GetAllAsync();
        return Ok(employees);
    }

    [Authorize(Roles = "Administrator,HRManager,Manager,Employee")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    [Authorize(Roles = "Administrator,HRManager,Manager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
    {
        var employee = await _employeeService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    [Authorize(Roles = "Administrator,HRManager,Manager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest request)
    {
        var employee = await _employeeService.UpdateAsync(id, request);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    [Authorize(Roles = "Administrator,HRManager,Manager")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _employeeService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
