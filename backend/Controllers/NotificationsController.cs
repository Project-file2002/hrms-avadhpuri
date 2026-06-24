using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly HRMSDbContext _context;
    public NotificationsController(HRMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] string? category, [FromQuery] bool? unread, [FromQuery] bool? starred)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var query = _context.Notifications.Where(n => n.UserId == userId);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(n => n.Category == category);
        if (unread == true)
            query = query.Where(n => !n.IsRead);
        if (starred == true)
            query = query.Where(n => n.IsStarred);

        var notifs = await query.OrderByDescending(n => n.CreatedAt).Take(100).ToListAsync();
        return Ok(notifs);
    }

    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var notifs = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        return Ok(new
        {
            unreadCount = notifs.Count(n => !n.IsRead),
            starredCount = notifs.Count(n => n.IsStarred),
            today = notifs.Where(n => n.CreatedAt.Date == today),
            yesterday = notifs.Where(n => n.CreatedAt.Date == yesterday),
            earlier = notifs.Where(n => n.CreatedAt.Date < yesterday),
            categories = notifs.GroupBy(n => n.Category).Select(g => new { category = g.Key, count = g.Count() })
        });
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var count = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        return Ok(new { count });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notif == null) return NotFound();
        notif.IsRead = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id}/star")]
    public async Task<IActionResult> ToggleStar(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notif == null) return NotFound();
        notif.IsStarred = !notif.IsStarred;
        await _context.SaveChangesAsync();
        return Ok(new { notif.IsStarred });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        return Ok();
    }
}
