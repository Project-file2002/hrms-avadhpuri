using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KnowledgeController : ControllerBase
{
    private readonly HRMSDbContext _context;
    public KnowledgeController(HRMSDbContext context) => _context = context;

    // ========== POLLS ==========

    [HttpGet("polls")]
    public async Task<IActionResult> GetPolls()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var polls = await _context.Polls
            .Include(p => p.CreatedBy)
            .Include(p => p.Options).ThenInclude(o => o.Votes)
            .OrderByDescending(p => p.IsActive).ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(polls.Select(p => new
        {
            p.Id, p.Question, p.MultiVote, p.IsActive, p.CreatedAt, p.ExpiresAt,
            CreatedByName = $"{p.CreatedBy.FirstName} {p.CreatedBy.LastName}",
            TotalVotes = p.Options.Sum(o => o.Votes.Count),
            UserVoted = p.Options.Any(o => o.Votes.Any(v => v.UserId == userId)),
            Options = p.Options.Select(o => new
            {
                o.Id, o.Text, VoteCount = o.Votes.Count,
                UserVotedThis = o.Votes.Any(v => v.UserId == userId)
            })
        }));
    }

    [HttpPost("polls")]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var poll = new Poll
        {
            Question = request.Question, MultiVote = request.MultiVote,
            ExpiresAt = request.ExpiresAt, CreatedById = user.EmployeeId.Value,
            Options = request.Options.Select(t => new PollOption { Text = t }).ToList()
        };
        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();
        return Ok(new { poll.Id });
    }

    [HttpPost("polls/{id}/vote")]
    public async Task<IActionResult> Vote(int id, [FromBody] PollVoteRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var poll = await _context.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.Id == id);
        if (poll == null) return NotFound();
        if (!poll.MultiVote)
        {
            var existing = await _context.PollVotes.Include(v => v.PollOption)
                .Where(v => v.PollOption.PollId == id && v.UserId == userId).ToListAsync();
            _context.PollVotes.RemoveRange(existing);
        }
        foreach (var optId in request.OptionIds)
        {
            _context.PollVotes.Add(new PollVote { PollOptionId = optId, UserId = userId });
        }
        await _context.SaveChangesAsync();
        return Ok();
    }

    [Authorize(Roles = "Administrator,HRManager")]
    [HttpDelete("polls/{id}")]
    public async Task<IActionResult> DeletePoll(int id)
    {
        var poll = await _context.Polls.FindAsync(id);
        if (poll == null) return NotFound();
        _context.Polls.Remove(poll);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ========== DISCUSSIONS ==========

    [HttpGet("threads")]
    public async Task<IActionResult> GetThreads([FromQuery] string? category)
    {
        var query = _context.DiscussionThreads
            .Include(t => t.CreatedBy)
            .Include(t => t.Replies)
            .AsQueryable();
        if (!string.IsNullOrEmpty(category) && category != "All")
            query = query.Where(t => t.Category == category);
        var threads = await query.OrderByDescending(t => t.IsPinned).ThenByDescending(t => t.CreatedAt).ToListAsync();
        return Ok(threads.Select(t => new
        {
            t.Id, t.Title, t.Content, t.Category, t.Tags, t.IsPinned, t.ViewCount, t.CreatedAt, t.UpdatedAt,
            CreatedByName = $"{t.CreatedBy.FirstName} {t.CreatedBy.LastName}",
            ReplyCount = t.Replies.Count
        }));
    }

    [HttpGet("threads/{id}")]
    public async Task<IActionResult> GetThread(int id)
    {
        var thread = await _context.DiscussionThreads
            .Include(t => t.CreatedBy)
            .Include(t => t.Replies).ThenInclude(r => r.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (thread == null) return NotFound();
        thread.ViewCount++;
        await _context.SaveChangesAsync();
        return Ok(new
        {
            thread.Id, thread.Title, thread.Content, thread.Category, thread.Tags, thread.IsPinned, thread.ViewCount, thread.CreatedAt, thread.UpdatedAt,
            CreatedByName = $"{thread.CreatedBy.FirstName} {thread.CreatedBy.LastName}",
            Replies = thread.Replies.OrderBy(r => r.CreatedAt).Select(r => new
            {
                r.Id, r.Content, r.CreatedAt, r.UpdatedAt,
                CreatedByName = $"{r.CreatedBy.FirstName} {r.CreatedBy.LastName}"
            })
        });
    }

    [HttpPost("threads")]
    public async Task<IActionResult> CreateThread([FromBody] CreateThreadRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();
        var thread = new DiscussionThread
        {
            Title = request.Title, Content = request.Content,
            Category = request.Category, Tags = request.Tags,
            CreatedById = user.EmployeeId.Value
        };
        _context.DiscussionThreads.Add(thread);
        await _context.SaveChangesAsync();
        return Ok(new { thread.Id });
    }

    [HttpPost("threads/{id}/reply")]
    public async Task<IActionResult> Reply(int id, [FromBody] ReplyRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();
        var reply = new DiscussionReply
        {
            ThreadId = id, Content = request.Content, CreatedById = user.EmployeeId.Value
        };
        _context.DiscussionReplies.Add(reply);
        var thread = await _context.DiscussionThreads.FindAsync(id);
        if (thread != null) thread.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { reply.Id });
    }

    [Authorize(Roles = "Administrator,HRManager")]
    [HttpDelete("threads/{id}")]
    public async Task<IActionResult> DeleteThread(int id)
    {
        var thread = await _context.DiscussionThreads.FindAsync(id);
        if (thread == null) return NotFound();
        _context.DiscussionThreads.Remove(thread);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ========== BIRTHDAYS & ANNIVERSARIES ==========

    [HttpGet("celebrations")]
    public async Task<IActionResult> GetCelebrations()
    {
        var today = DateTime.UtcNow;
        var employees = await _context.Employees.Where(e => !e.IsDeleted).ToListAsync();

        var birthdays = employees
            .Where(e => e.DateOfBirth != null)
            .Select(e => new
            {
                e.Id, Name = $"{e.FirstName} {e.LastName}", e.Position,
                Date = e.DateOfBirth,
                Upcoming = GetNextOccurrence(e.DateOfBirth!.Value, today),
                Type = "Birthday"
            })
            .Where(e => (e.Upcoming - today).TotalDays >= 0 && (e.Upcoming - today).TotalDays <= 30)
            .OrderBy(e => e.Upcoming).Take(5);

        var anniversaries = employees
            .Where(e => e.DateOfJoining != null)
            .Select(e => new
            {
                e.Id, Name = $"{e.FirstName} {e.LastName}", e.Position,
                Date = e.DateOfJoining,
                Upcoming = GetNextOccurrence(e.DateOfJoining!.Value, today),
                Years = today.Year - e.DateOfJoining.Value.Year,
                Type = "Anniversary"
            })
            .Where(e => (e.Upcoming - today).TotalDays >= 0 && (e.Upcoming - today).TotalDays <= 30)
            .OrderBy(e => e.Upcoming).Take(5);

        return Ok(new { Birthdays = birthdays, Anniversaries = anniversaries });
    }

    private static DateTime GetNextOccurrence(DateTime date, DateTime from)
    {
        var next = new DateTime(from.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        if (next < from.Date) next = next.AddYears(1);
        return next;
    }
}

public record CreatePollRequest(string Question, bool MultiVote, DateTime? ExpiresAt, List<string> Options);
public record PollVoteRequest(List<int> OptionIds);
public record CreateThreadRequest(string Title, string Content, string Category, string? Tags);
public record ReplyRequest(string Content);
