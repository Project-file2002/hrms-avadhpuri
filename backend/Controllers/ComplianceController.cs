using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager,Manager")]
public class ComplianceController : ControllerBase
{
    private readonly HRMSDbContext _context;
    public ComplianceController(HRMSDbContext context) => _context = context;

    // === AUDIT LOG ===

    [HttpGet("audit-log")]
    public async Task<IActionResult> GetAuditLog([FromQuery] string? entityType, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _context.AuditLogs.Include(a => a.User).AsQueryable();
        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);
        var total = await query.CountAsync();
        var logs = await query.OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new
            {
                a.Id, a.Action, a.EntityType, a.EntityId, a.Details, a.Timestamp,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "System"
            }).ToListAsync();
        return Ok(new { items = logs, total, page, pageSize });
    }

    // === COMPLIANCE RECORDS ===

    [HttpGet("records")]
    public async Task<IActionResult> GetRecords([FromQuery] string? status)
    {
        var query = _context.ComplianceRecords
            .Include(c => c.AssignedTo)
            .Include(c => c.CompletedBy).AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(c => c.Status == status);
        var records = await query.OrderByDescending(c => c.DueDate).ToListAsync();
        return Ok(records.Select(c => new
        {
            c.Id, c.Title, c.Category, c.Regulation, c.Status, c.Notes, c.DueDate, c.CompletedAt, c.CreatedAt,
            AssignedToName = c.AssignedTo != null ? $"{c.AssignedTo.FirstName} {c.AssignedTo.LastName}" : null,
            CompletedByName = c.CompletedBy != null ? $"{c.CompletedBy.FirstName} {c.CompletedBy.LastName}" : null
        }));
    }

    [HttpPost("records")]
    public async Task<IActionResult> CreateRecord([FromBody] CreateComplianceRecord request)
    {
        var record = new ComplianceRecord
        {
            Title = request.Title, Category = request.Category,
            Regulation = request.Regulation, Notes = request.Notes,
            DueDate = request.DueDate, AssignedToId = request.AssignedToId
        };
        _context.ComplianceRecords.Add(record);
        await _context.SaveChangesAsync();
        return Ok(new { record.Id });
    }

    [HttpPut("records/{id}")]
    public async Task<IActionResult> UpdateRecord(int id, [FromBody] UpdateComplianceRecord request)
    {
        var record = await _context.ComplianceRecords.FindAsync(id);
        if (record == null) return NotFound();

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);

        record.Status = request.Status;
        record.Notes = request.Notes ?? record.Notes;
        if (request.Status == "Completed")
        {
            record.CompletedAt = DateTime.UtcNow;
            record.CompletedById = user?.EmployeeId;
        }
        await _context.SaveChangesAsync();

        _context.AuditLogs.Add(new AuditLog
        {
            Action = $"Compliance '{record.Title}' marked as {request.Status}",
            EntityType = "ComplianceRecord", EntityId = record.Id,
            UserId = userId, Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return Ok(record);
    }

    [HttpDelete("records/{id}")]
    public async Task<IActionResult> DeleteRecord(int id)
    {
        var record = await _context.ComplianceRecords.FindAsync(id);
        if (record == null) return NotFound();
        _context.ComplianceRecords.Remove(record);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // === DATA PRIVACY ===

    [HttpGet("privacy-logs")]
    public async Task<IActionResult> GetPrivacyLogs([FromQuery] int? employeeId)
    {
        var query = _context.DataPrivacyLogs
            .Include(l => l.Employee)
            .Include(l => l.User).AsQueryable();
        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId);
        var logs = await query.OrderByDescending(l => l.Timestamp).Take(100).ToListAsync();
        return Ok(logs.Select(l => new
        {
            l.Id, l.Action, l.DataCategory, l.Details, l.ConsentStatus, l.Timestamp, l.IpAddress,
            EmployeeName = l.Employee != null ? $"{l.Employee.FirstName} {l.Employee.LastName}" : null,
            UserName = l.User != null ? $"{l.User.FirstName} {l.User.LastName}" : null
        }));
    }

    [HttpPost("privacy-logs")]
    public async Task<IActionResult> AddPrivacyLog([FromBody] AddPrivacyLogRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var log = new DataPrivacyLog
        {
            Action = request.Action, DataCategory = request.DataCategory,
            Details = request.Details, ConsentStatus = request.ConsentStatus,
            EmployeeId = request.EmployeeId, UserId = userId,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };
        _context.DataPrivacyLogs.Add(log);
        await _context.SaveChangesAsync();
        return Ok(new { log.Id });
    }

    // === DASHBOARD ===

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var total = await _context.ComplianceRecords.CountAsync();
        var pending = await _context.ComplianceRecords.CountAsync(c => c.Status == "Pending");
        var completed = await _context.ComplianceRecords.CountAsync(c => c.Status == "Completed");
        var overdue = await _context.ComplianceRecords.CountAsync(c => c.Status != "Completed" && c.DueDate < DateTime.UtcNow);
        var auditCount = await _context.AuditLogs.CountAsync();
        var privacyCount = await _context.DataPrivacyLogs.CountAsync();

        var categories = await _context.ComplianceRecords
            .GroupBy(c => c.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new { totalRecords = total, pending, completed, overdue, auditLogCount = auditCount, privacyLogCount = privacyCount, categories });
    }
}

public record CreateComplianceRecord(string Title, string Category, string Regulation, string? Notes, DateTime? DueDate, int? AssignedToId);
public record UpdateComplianceRecord(string Status, string? Notes);
public record AddPrivacyLogRequest(string Action, string DataCategory, string? Details, string ConsentStatus, int? EmployeeId);
