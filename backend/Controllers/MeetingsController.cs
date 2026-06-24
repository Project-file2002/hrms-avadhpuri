using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeetingsController : ControllerBase
{
    private readonly HRMSDbContext _context;
    private readonly IAIService _aiService;

    public MeetingsController(HRMSDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? departmentId, [FromQuery] string? filter)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var query = _context.Meetings
            .Include(m => m.Organizer)
            .Include(m => m.Department)
            .Include(m => m.Participants).ThenInclude(p => p.Employee)
            .AsQueryable();

        if (departmentId.HasValue)
            query = query.Where(m => m.DepartmentId == departmentId);

        var now = DateTime.UtcNow;
        query = filter switch
        {
            "today" => query.Where(m => m.StartTime.Date == now.Date),
            "upcoming" => query.Where(m => m.StartTime > now && m.Status == "Scheduled"),
            "pending-invites" => query.Where(m => m.Participants.Any(p =>
                p.EmployeeId == user!.EmployeeId && p.ResponseStatus == "Pending")),
            _ => query
        };

        var meetings = await query.OrderBy(m => m.StartTime).ToListAsync();
        return Ok(meetings.Select(MapMeeting));
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var empId = user?.EmployeeId;
        var now = DateTime.UtcNow;
        var todayEnd = now.Date.AddDays(1);

        var today = await _context.Meetings
            .Include(m => m.Organizer).Include(m => m.Department)
            .Where(m => m.StartTime >= now.Date && m.StartTime < todayEnd && m.Status == "Scheduled")
            .OrderBy(m => m.StartTime).Take(10).ToListAsync();

        var upcoming = await _context.Meetings
            .Include(m => m.Organizer).Include(m => m.Department)
            .Where(m => m.StartTime > now && m.Status == "Scheduled")
            .OrderBy(m => m.StartTime).Take(10).ToListAsync();

        var pendingInvites = empId.HasValue
            ? await _context.MeetingParticipants.CountAsync(p =>
                p.EmployeeId == empId && p.ResponseStatus == "Pending" &&
                p.Meeting.StartTime > now && p.Meeting.Status == "Scheduled")
            : 0;

        return Ok(new
        {
            todayMeetings = today.Select(MapMeeting),
            upcomingMeetings = upcoming.Select(MapMeeting),
            pendingInvitations = pendingInvites,
            totalScheduled = await _context.Meetings.CountAsync(m => m.Status == "Scheduled" && m.StartTime > now)
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var meeting = await _context.Meetings
            .Include(m => m.Organizer).Include(m => m.Department)
            .Include(m => m.Participants).ThenInclude(p => p.Employee)
            .Include(m => m.Tasks).ThenInclude(t => t.AssignedTo)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (meeting == null) return NotFound();
        return Ok(MapMeetingDetail(meeting));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMeetingRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var meeting = await _context.Meetings
            .Include(m => m.Participants).ThenInclude(p => p.Employee)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (meeting == null) return NotFound();
        if (meeting.Status != "Scheduled") return BadRequest("Can only edit scheduled meetings");

        var isAdmin = User.IsInRole("Administrator") || User.IsInRole("HRManager");
        if (meeting.OrganizerId != user.EmployeeId && !isAdmin) return Forbid();

        meeting.Title = request.Title;
        meeting.Description = request.Description;
        meeting.Agenda = request.Agenda;
        meeting.MeetingType = request.MeetingType;
        meeting.Priority = request.Priority ?? "Normal";
        meeting.Timezone = request.Timezone ?? "Asia/Kolkata";
        meeting.Recurrence = request.Recurrence;
        meeting.Location = request.Location;
        meeting.OnlineLink = request.OnlineLink;
        meeting.StartTime = request.StartTime.ToUniversalTime();
        meeting.EndTime = request.EndTime.ToUniversalTime();
        meeting.DepartmentId = request.DepartmentId;
        meeting.UpdatedAt = DateTime.UtcNow;

        _context.MeetingParticipants.RemoveRange(meeting.Participants);
        meeting.Participants = (request.ParticipantIds ?? new List<int>()).Select(pid => new MeetingParticipant
        {
            EmployeeId = pid,
            ResponseStatus = pid == meeting.OrganizerId ? "Accepted" : "Pending",
            IsOptional = (request.OptionalGuestIds ?? new List<int>()).Contains(pid)
        }).ToList();

        if (!meeting.Participants.Any(p => p.EmployeeId == meeting.OrganizerId))
            meeting.Participants.Add(new MeetingParticipant { EmployeeId = meeting.OrganizerId, ResponseStatus = "Accepted" });

        await _context.SaveChangesAsync();
        return Ok(MapMeeting(meeting));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var meeting = await _context.Meetings.FindAsync(id);
        if (meeting == null) return NotFound();
        if (meeting.Status != "Scheduled") return BadRequest("Can only delete scheduled meetings");

        var isAdmin = User.IsInRole("Administrator") || User.IsInRole("HRManager");
        if (meeting.OrganizerId != user.EmployeeId && !isAdmin) return Forbid();

        _context.Meetings.Remove(meeting);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMeetingRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var organizerId = request.OrganizerId ?? user.EmployeeId.Value;
        var meeting = new Meeting
        {
            Title = request.Title,
            Description = request.Description,
            Agenda = request.Agenda,
            MeetingType = request.MeetingType,
            Priority = request.Priority ?? "Normal",
            Timezone = request.Timezone ?? "Asia/Kolkata",
            Recurrence = request.Recurrence,
            Location = request.Location,
            OnlineLink = request.OnlineLink,
            StartTime = request.StartTime.ToUniversalTime(),
            EndTime = request.EndTime.ToUniversalTime(),
            OrganizerId = organizerId,
            DepartmentId = request.DepartmentId,
            Participants = (request.ParticipantIds ?? new List<int>()).Select(pid => new MeetingParticipant
            {
                EmployeeId = pid,
                ResponseStatus = pid == organizerId ? "Accepted" : "Pending",
                IsOptional = (request.OptionalGuestIds ?? new List<int>()).Contains(pid)
            }).ToList()
        };

        if (!meeting.Participants.Any(p => p.EmployeeId == organizerId))
            meeting.Participants.Add(new MeetingParticipant { EmployeeId = organizerId, ResponseStatus = "Accepted" });

        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync();

        await NotifyParticipantsAsync(meeting.Id, meeting.Title, request.ParticipantIds ?? new List<int>());
        return CreatedAtAction(nameof(GetById), new { id = meeting.Id }, MapMeeting(meeting));
    }

    [HttpPut("{id}/respond")]
    public async Task<IActionResult> Respond(int id, [FromBody] MeetingRespondRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var participant = await _context.MeetingParticipants
            .FirstOrDefaultAsync(p => p.MeetingId == id && p.EmployeeId == user.EmployeeId);
        if (participant == null) return NotFound();
        participant.ResponseStatus = request.Accepted ? "Accepted" : "Declined";
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteMeetingRequest request)
    {
        var meeting = await _context.Meetings.FindAsync(id);
        if (meeting == null) return NotFound();
        meeting.Status = "Completed";
        meeting.AiSummary = request.Notes;
        meeting.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            var summary = await _aiService.SummarizeMeetingAsync(meeting.Title, request.Notes);
            meeting.AiSummary = summary;
        }

        if (request.ActionItems?.Count > 0)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            foreach (var item in request.ActionItems)
            {
                _context.WorkTasks.Add(new WorkTask
                {
                    Title = item.Title,
                    Description = item.Description,
                    AssignedToId = item.AssignedToId,
                    AssignedById = user?.EmployeeId,
                    MeetingId = id,
                    DueDate = item.DueDate,
                    Priority = item.Priority ?? "Normal"
                });
            }
        }

        await _context.SaveChangesAsync();
        return Ok(MapMeeting(meeting));
    }

    [HttpPost("ai/agenda")]
    public async Task<IActionResult> GenerateAgenda([FromBody] AiAgendaRequest request)
    {
        var result = await _aiService.GenerateMeetingAgendaAsync(request.Title, request.MeetingType, request.Description);
        return Ok(new { agenda = result });
    }

    [HttpPost("ai/suggest-time")]
    public async Task<IActionResult> SuggestTime([FromBody] SuggestTimeRequest request)
    {
        var participantIds = request.ParticipantIds ?? new List<int>();
        var conflicts = new List<object>();
        var proposedStart = request.ProposedStart.ToUniversalTime();
        var proposedEnd = request.ProposedEnd.ToUniversalTime();

        foreach (var empId in participantIds)
        {
            var overlapping = await _context.Meetings
                .Where(m => m.Status == "Scheduled" &&
                    m.StartTime < proposedEnd && m.EndTime > proposedStart &&
                    (m.OrganizerId == empId || m.Participants.Any(p => p.EmployeeId == empId)))
                .Select(m => new { m.Title, m.StartTime, m.EndTime })
                .FirstOrDefaultAsync();
            if (overlapping != null)
            {
                var emp = await _context.Employees.FindAsync(empId);
                conflicts.Add(new
                {
                    employeeId = empId,
                    employeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : "Unknown",
                    conflictWith = overlapping.Title,
                    conflictStart = overlapping.StartTime,
                    conflictEnd = overlapping.EndTime
                });
            }
        }

        var duration = await _aiService.SuggestMeetingDurationAsync(request.MeetingType, participantIds.Count);
        var suggestedStart = conflicts.Count == 0 ? proposedStart : proposedStart.AddHours(1);

        return Ok(new
        {
            hasConflicts = conflicts.Count > 0,
            conflicts,
            suggestedDurationMinutes = duration,
            suggestedStart,
            suggestedEnd = suggestedStart.AddMinutes(duration)
        });
    }

    [HttpPost("ai/mom-template")]
    public async Task<IActionResult> GenerateMomTemplate([FromBody] AiAgendaRequest request)
    {
        var result = await _aiService.GenerateMomTemplateAsync(request.Title, request.MeetingType);
        return Ok(new { template = result });
    }

    private async Task NotifyParticipantsAsync(int meetingId, string title, List<int> participantIds)
    {
        var users = await _context.Users
            .Where(u => u.EmployeeId != null && participantIds.Contains(u.EmployeeId.Value))
            .ToListAsync();
        foreach (var u in users)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = u.Id,
                Title = "Meeting Invitation",
                Message = $"You are invited to: {title}",
                Type = "Meeting",
                Category = "Reminder",
                Source = "Meeting",
                Priority = "Normal",
                Link = "/meetings"
            });
        }
        await _context.SaveChangesAsync();
    }

    private static object MapMeeting(Meeting m) => new
    {
        m.Id, m.OrganizerId, m.Title, m.Description, m.Agenda, m.MeetingType, m.Status, m.Priority,
        m.Timezone, m.Recurrence, m.Location, m.OnlineLink, m.StartTime, m.EndTime,
        m.DepartmentId,
        DepartmentName = m.Department?.Name,
        OrganizerName = $"{m.Organizer.FirstName} {m.Organizer.LastName}",
        ParticipantCount = m.Participants?.Count ?? 0,
        Participants = m.Participants?.Select(p => new
        {
            p.Id, p.EmployeeId,
            EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
            p.ResponseStatus, p.IsOptional
        })
    };

    private static object MapMeetingDetail(Meeting m) => new
    {
        m.Id, m.Title, m.Description, m.Agenda, m.MeetingType, m.Status, m.Priority,
        m.Timezone, m.Recurrence, m.Location, m.OnlineLink, m.StartTime, m.EndTime,
        m.AiSummary, m.AiMomTemplate, m.DepartmentId,
        DepartmentName = m.Department?.Name,
        OrganizerName = $"{m.Organizer.FirstName} {m.Organizer.LastName}",
        Participants = m.Participants.Select(p => new
        {
            p.Id, p.EmployeeId,
            EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
            p.ResponseStatus, p.IsOptional
        }),
        Tasks = m.Tasks.Select(t => new
        {
            t.Id, t.Title, t.Status, t.Priority, t.DueDate,
            AssignedToName = $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}"
        })
    };
}

public class UpdateMeetingRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Agenda { get; set; }
    public string MeetingType { get; set; } = "Team";
    public string? Priority { get; set; }
    public string? Timezone { get; set; }
    public string? Recurrence { get; set; }
    public string? Location { get; set; }
    public string? OnlineLink { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? DepartmentId { get; set; }
    public List<int>? ParticipantIds { get; set; }
    public List<int>? OptionalGuestIds { get; set; }
}

public class CreateMeetingRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Agenda { get; set; }
    public string MeetingType { get; set; } = "Team";
    public string? Priority { get; set; }
    public string? Timezone { get; set; }
    public string? Recurrence { get; set; }
    public string? Location { get; set; }
    public string? OnlineLink { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? OrganizerId { get; set; }
    public int? DepartmentId { get; set; }
    public List<int>? ParticipantIds { get; set; }
    public List<int>? OptionalGuestIds { get; set; }
}

public class MeetingRespondRequest
{
    public bool Accepted { get; set; }
}

public class CompleteMeetingRequest
{
    public string? Notes { get; set; }
    public List<MeetingActionItem>? ActionItems { get; set; }
}

public class MeetingActionItem
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Priority { get; set; }
}

public class AiAgendaRequest
{
    public string Title { get; set; } = string.Empty;
    public string MeetingType { get; set; } = "Team";
    public string? Description { get; set; }
}

public class SuggestTimeRequest
{
    public string MeetingType { get; set; } = "Team";
    public DateTime ProposedStart { get; set; }
    public DateTime ProposedEnd { get; set; }
    public List<int>? ParticipantIds { get; set; }
}
