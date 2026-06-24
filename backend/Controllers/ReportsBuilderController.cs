using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.NoCode;
using HRMS.API.Models.Entities;
using System.Text.Json;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/reports-builder")]
[Authorize(Roles = "Administrator,HRManager,Manager,PayrollStaff")]
public class ReportsBuilderController : ControllerBase
{
    private readonly HRMSDbContext _context;

    public ReportsBuilderController(HRMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var reports = await _context.ReportDefinitions
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(reports.Select(r => new ReportDefinitionDto
        {
            Id = r.Id,
            Title = r.Title,
            DataSource = r.DataSource,
            Columns = r.Columns,
            Filters = r.Filters,
            GroupBy = r.GroupBy,
            CreatedAt = r.CreatedAt
        }));
    }

    [HttpGet("datasources")]
    public IActionResult GetDataSources()
    {
        var sources = new Dictionary<string, List<string>>
        {
            ["Employee"] = new() { "Id", "EmployeeCode", "FirstName", "LastName", "Email", "Phone", "Position", "DepartmentName", "ManagerName", "Status", "DateOfJoining", "Gender" },
            ["Leave"] = new() { "Id", "EmployeeName", "LeaveTypeName", "StartDate", "EndDate", "Status", "Reason", "CreatedAt" },
            ["Attendance"] = new() { "Id", "EmployeeId", "Date", "CheckInTime", "CheckOutTime", "Status" },
            ["Performance"] = new() { "Id", "Title", "EmployeeName", "ReviewerName", "CycleName", "OverallScore", "Status", "StartDate", "EndDate" },
            ["Department"] = new() { "Id", "Name", "HeadName", "EmployeeCount", "Description" },
            ["Recruitment"] = new() { "Id", "Title", "Status", "CandidateCount", "CreatedAt" }
        };
        return Ok(sources);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportDefinitionRequest request)
    {
        var report = new ReportDefinition
        {
            Title = request.Title,
            DataSource = request.DataSource,
            Columns = request.Columns,
            Filters = request.Filters,
            GroupBy = request.GroupBy
        };
        _context.ReportDefinitions.Add(report);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var report = await _context.ReportDefinitions.FindAsync(id);
        if (report == null) return NotFound();
        return Ok(report);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var report = await _context.ReportDefinitions.FindAsync(id);
        if (report == null) return NotFound();
        _context.ReportDefinitions.Remove(report);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/run")]
    public async Task<IActionResult> Run(int id)
    {
        var report = await _context.ReportDefinitions.FindAsync(id);
        if (report == null) return NotFound();

        var columns = JsonSerializer.Deserialize<List<string>>(report.Columns) ?? new();
        var result = new ReportResultDto
        {
            Title = report.Title,
            DataSource = report.DataSource,
            ColumnNames = columns
        };

        switch (report.DataSource)
        {
            case "Employee":
                var employees = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Manager)
                    .Where(e => !e.IsDeleted)
                    .ToListAsync();
                result.Rows = employees.Select(e => ToDictionary(e, columns, new()
                {
                    ["DepartmentName"] = e.Department?.Name,
                    ["ManagerName"] = e.Manager != null ? $"{e.Manager.FirstName} {e.Manager.LastName}" : null
                })).ToList();
                break;

            case "Leave":
                var leaves = await _context.LeaveRequests
                    .Include(l => l.Employee)
                    .Include(l => l.LeaveType)
                    .ToListAsync();
                result.Rows = leaves.Select(l => ToDictionary(l, columns, new()
                {
                    ["EmployeeName"] = $"{l.Employee.FirstName} {l.Employee.LastName}",
                    ["LeaveTypeName"] = l.LeaveType.Name
                })).ToList();
                break;

            case "Attendance":
                var attendance = await _context.AttendanceRecords.ToListAsync();
                result.Rows = attendance.Select(a => ToDictionary(a, columns, null)).ToList();
                break;

            case "Performance":
                var reviews = await _context.PerformanceReviews
                    .Include(r => r.Employee)
                    .Include(r => r.Reviewer)
                    .Include(r => r.Cycle)
                    .ToListAsync();
                result.Rows = reviews.Select(r => ToDictionary(r, columns, new()
                {
                    ["EmployeeName"] = $"{r.Employee.FirstName} {r.Employee.LastName}",
                    ["ReviewerName"] = $"{r.Reviewer.FirstName} {r.Reviewer.LastName}",
                    ["CycleName"] = r.Cycle.Name
                })).ToList();
                break;

            case "Department":
                var depts = await _context.Departments
                    .Include(d => d.Head)
                    .Include(d => d.Employees)
                    .Where(d => !d.IsDeleted)
                    .ToListAsync();
                result.Rows = depts.Select(d => ToDictionary(d, columns, new()
                {
                    ["HeadName"] = d.Head != null ? $"{d.Head.FirstName} {d.Head.LastName}" : null,
                    ["EmployeeCount"] = d.Employees.Count(e => !e.IsDeleted)
                })).ToList();
                break;

            case "Recruitment":
                var jobs = await _context.JobRequisitions
                    .Include(j => j.Candidates)
                    .ToListAsync();
                result.Rows = jobs.Select(j => ToDictionary(j, columns, new()
                {
                    ["CandidateCount"] = j.Candidates.Count
                })).ToList();
                break;
        }

        return Ok(result);
    }

    private static Dictionary<string, object?> ToDictionary(object entity, List<string> columns, Dictionary<string, object?>? overrides)
    {
        var dict = new Dictionary<string, object?>();
        var type = entity.GetType();

        foreach (var col in columns)
        {
            if (overrides?.ContainsKey(col) == true)
            {
                dict[col] = overrides[col];
                continue;
            }

            var prop = type.GetProperty(col);
            if (prop != null)
            {
                dict[col] = prop.GetValue(entity);
            }
        }

        return dict;
    }
}
