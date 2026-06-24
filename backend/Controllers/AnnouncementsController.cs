using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnnouncementsController : ControllerBase
{
    private readonly HRMSDbContext _context;
    public AnnouncementsController(HRMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? scope)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var query = _context.Announcements
            .Include(a => a.CreatedBy)
            .Include(a => a.Department)
            .Include(a => a.Reads.Where(r => r.UserId == userId))
            .AsQueryable();

        if (!string.IsNullOrEmpty(scope))
            query = query.Where(a => a.Scope == scope);

        var items = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return Ok(items.Select(a => MapAnnouncement(a, userId)));
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,HRManager,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var announcement = new Announcement
        {
            Title = request.Title,
            Content = request.Content,
            Scope = request.Scope ?? "Company",
            Priority = request.Priority ?? "Normal",
            AcknowledgementRequired = request.AcknowledgementRequired,
            DepartmentId = request.DepartmentId,
            CreatedById = user.EmployeeId.Value,
            ExpiresAt = request.ExpiresAt
        };
        _context.Announcements.Add(announcement);

        var targetUsers = await _context.Users.ToListAsync();
        foreach (var u in targetUsers)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = u.Id,
                Title = $"Announcement: {request.Title}",
                Message = request.Content.Length > 120 ? request.Content[..120] + "..." : request.Content,
                Type = "Announcement",
                Category = request.Priority == "Urgent" ? "Urgent" : "Information",
                Source = "Announcements",
                Priority = request.Priority ?? "Normal",
                Link = "/announcements"
            });
        }

        await _context.SaveChangesAsync();
        return Ok(MapAnnouncement(announcement, userId));
    }

    [HttpPut("{id}/acknowledge")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var existing = await _context.AnnouncementReads
            .FirstOrDefaultAsync(r => r.AnnouncementId == id && r.UserId == userId);

        if (existing == null)
        {
            _context.AnnouncementReads.Add(new AnnouncementRead
            {
                AnnouncementId = id,
                UserId = userId,
                AcknowledgedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.AcknowledgedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var existing = await _context.AnnouncementReads
            .FirstOrDefaultAsync(r => r.AnnouncementId == id && r.UserId == userId);

        if (existing == null)
        {
            _context.AnnouncementReads.Add(new AnnouncementRead
            {
                AnnouncementId = id,
                UserId = userId
            });
            await _context.SaveChangesAsync();
        }
        return Ok();
    }

    private static object MapAnnouncement(Announcement a, int userId) => new
    {
        a.Id, a.Title, a.Content, a.Scope, a.Priority,
        a.AcknowledgementRequired, a.CreatedAt, a.ExpiresAt,
        DepartmentName = a.Department?.Name,
        CreatedByName = $"{a.CreatedBy.FirstName} {a.CreatedBy.LastName}",
        IsRead = a.Reads.Any(r => r.UserId == userId),
        IsAcknowledged = a.Reads.Any(r => r.UserId == userId && r.AcknowledgedAt != null),
        ReadCount = a.Reads.Count
    };
}

public class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Scope { get; set; }
    public string? Priority { get; set; }
    public bool AcknowledgementRequired { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
