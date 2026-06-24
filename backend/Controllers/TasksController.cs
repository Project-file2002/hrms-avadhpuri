using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly HRMSDbContext _context;
    public TasksController(HRMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? employeeId, [FromQuery] string? status)
    {
        var query = _context.WorkTasks
            .Include(t => t.AssignedTo)
            .Include(t => t.AssignedBy)
            .Include(t => t.Meeting)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(t => t.AssignedToId == employeeId);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return Ok(tasks.Select(MapTask));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        var task = new WorkTask
        {
            Title = request.Title,
            Description = request.Description,
            AssignedToId = request.AssignedToId,
            AssignedById = user?.EmployeeId,
            MeetingId = request.MeetingId,
            DueDate = request.DueDate,
            Priority = request.Priority ?? "Normal"
        };
        _context.WorkTasks.Add(task);

        var assigneeUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == request.AssignedToId);
        if (assigneeUser != null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = assigneeUser.Id,
                Title = "New Task Assigned",
                Message = request.Title,
                Type = "Task",
                Category = "Task",
                Source = request.MeetingId.HasValue ? "Meeting" : "System",
                Priority = request.Priority ?? "Normal",
                Link = "/tasks"
            });
        }

        await _context.SaveChangesAsync();
        return Ok(MapTask(task));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        var task = await _context.WorkTasks.FindAsync(id);
        if (task == null) return NotFound();
        task.Title = request.Title;
        task.Description = request.Description;
        task.AssignedToId = request.AssignedToId;
        task.Priority = request.Priority ?? "Normal";
        task.DueDate = request.DueDate;
        await _context.SaveChangesAsync();
        return Ok(MapTask(task));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _context.WorkTasks.FindAsync(id);
        if (task == null) return NotFound();
        _context.WorkTasks.Remove(task);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var task = await _context.WorkTasks.FindAsync(id);
        if (task == null) return NotFound();
        task.Status = "Completed";
        task.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(MapTask(task));
    }

    private static object MapTask(WorkTask t) => new
    {
        t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
        t.MeetingId, t.CreatedAt, t.CompletedAt,
        AssignedToName = t.AssignedTo != null ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}" : null,
        AssignedByName = t.AssignedBy != null ? $"{t.AssignedBy.FirstName} {t.AssignedBy.LastName}" : null,
        MeetingTitle = t.Meeting?.Title
    };
}

public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Priority { get; set; }
}

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssignedToId { get; set; }
    public int? MeetingId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Priority { get; set; }
}
