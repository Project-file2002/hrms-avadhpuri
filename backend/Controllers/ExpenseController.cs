using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpenseController : ControllerBase
{
    private readonly HRMSDbContext _context;

    public ExpenseController(HRMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? employeeId)
    {
        var query = _context.ExpenseReports
            .Include(e => e.Employee)
            .Include(e => e.ReviewedBy)
            .Include(e => e.LineItems)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(e => e.EmployeeId == employeeId);

        var reports = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
        return Ok(reports.Select(MapReport));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var report = await _context.ExpenseReports
            .Include(e => e.Employee)
            .Include(e => e.ReviewedBy)
            .Include(e => e.LineItems)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (report == null) return NotFound();
        return Ok(MapReport(report));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var report = new ExpenseReport
        {
            Title = request.Title,
            Description = request.Description,
            TotalAmount = request.LineItems.Sum(li => li.Amount),
            EmployeeId = user.EmployeeId.Value,
            LineItems = request.LineItems.Select(li => new ExpenseLineItem
            {
                Category = li.Category, Description = li.Description,
                Amount = li.Amount, ExpenseDate = li.ExpenseDate
            }).ToList()
        };
        _context.ExpenseReports.Add(report);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
    }

    [Authorize(Roles = "Administrator,HRManager,Manager")]
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ReviewExpenseRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        var report = await _context.ExpenseReports.FindAsync(id);
        if (report == null) return NotFound();
        report.Status = request.Approved ? "Approved" : "Rejected";
        report.ReviewedById = user?.EmployeeId;
        report.ReviewNotes = request.Notes;
        report.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(report);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var report = await _context.ExpenseReports
            .Include(e => e.LineItems)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (report == null) return NotFound();
        if (report.Status != "Pending") return BadRequest("Can only edit pending reports");

        var isAdmin = User.IsInRole("Administrator") || User.IsInRole("HRManager");
        if (report.EmployeeId != user.EmployeeId && !isAdmin) return Forbid();

        report.Title = request.Title;
        report.Description = request.Description;
        report.TotalAmount = request.LineItems.Sum(li => li.Amount);
        _context.ExpenseLineItems.RemoveRange(report.LineItems);
        report.LineItems = request.LineItems.Select(li => new ExpenseLineItem
        {
            Category = li.Category, Description = li.Description,
            Amount = li.Amount, ExpenseDate = li.ExpenseDate
        }).ToList();
        await _context.SaveChangesAsync();
        return Ok(MapReport(report));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var report = await _context.ExpenseReports.FindAsync(id);
        if (report == null) return NotFound();
        if (report.Status != "Pending") return BadRequest("Can only delete pending reports");

        var isAdmin = User.IsInRole("Administrator") || User.IsInRole("HRManager");
        if (report.EmployeeId != user.EmployeeId && !isAdmin) return Forbid();

        _context.ExpenseReports.Remove(report);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static object MapReport(ExpenseReport r) => new
    {
        r.Id, r.EmployeeId, r.Title, r.Description, r.TotalAmount, r.Status, r.CreatedAt, r.ReviewedAt, r.ReviewNotes,
        EmployeeName = $"{r.Employee.FirstName} {r.Employee.LastName}",
        ReviewedByName = r.ReviewedBy != null ? $"{r.ReviewedBy.FirstName} {r.ReviewedBy.LastName}" : null,
        LineItems = r.LineItems.Select(li => new { li.Id, li.Category, li.Description, li.Amount, li.ExpenseDate })
    };
}

public class CreateExpenseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateLineItem> LineItems { get; set; } = new();
}

public class CreateLineItem
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
}

public class UpdateExpenseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateLineItem> LineItems { get; set; } = new();
}

public class ReviewExpenseRequest
{
    public bool Approved { get; set; }
    public string? Notes { get; set; }
}
