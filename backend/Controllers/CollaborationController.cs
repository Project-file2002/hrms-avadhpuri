using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CollaborationController : ControllerBase
{
    private readonly HRMSDbContext _context;
    public CollaborationController(HRMSDbContext context) => _context = context;

    [HttpGet("channels")]
    public async Task<IActionResult> GetChannels()
    {
        var channels = await _context.CollaborationChannels
            .Include(c => c.Department)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var result = new List<object>();
        foreach (var c in channels)
        {
            var lastMsg = await _context.ChannelMessages
                .Include(m => m.Author)
                .Where(m => m.ChannelId == c.Id)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();
            var msgCount = await _context.ChannelMessages.CountAsync(m => m.ChannelId == c.Id);

            result.Add(new
            {
                c.Id, c.Name, c.Description, c.ChannelType,
                DepartmentName = c.Department?.Name,
                MessageCount = msgCount,
                LastMessage = lastMsg != null ? new
                {
                    lastMsg.Content,
                    lastMsg.CreatedAt,
                    AuthorName = $"{lastMsg.Author.FirstName} {lastMsg.Author.LastName}"
                } : null
            });
        }

        return Ok(result);
    }

    [HttpGet("channels/{id}/messages")]
    public async Task<IActionResult> GetMessages(int id)
    {
        var messages = await _context.ChannelMessages
            .Include(m => m.Author)
            .Where(m => m.ChannelId == id)
            .OrderByDescending(m => m.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Ok(messages.Select(m => new
        {
            m.Id, m.Content, m.MessageType, m.IsPinned, m.CreatedAt,
            AuthorName = $"{m.Author.FirstName} {m.Author.LastName}",
            AuthorId = m.AuthorId
        }).Reverse());
    }

    [HttpPost("channels")]
    public async Task<IActionResult> CreateChannel([FromBody] CreateChannelRequest request)
    {
        var channel = new CollaborationChannel
        {
            Name = request.Name,
            Description = request.Description,
            ChannelType = request.ChannelType ?? "General",
            DepartmentId = request.DepartmentId
        };
        _context.CollaborationChannels.Add(channel);
        await _context.SaveChangesAsync();
        return Ok(new { channel.Id, channel.Name, channel.Description, channel.ChannelType, channel.DepartmentId });
    }

    [HttpPut("channels/{channelId}/messages/{messageId}")]
    public async Task<IActionResult> EditMessage(int channelId, int messageId, [FromBody] EditMessageRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var message = await _context.ChannelMessages.FirstOrDefaultAsync(m => m.Id == messageId && m.ChannelId == channelId);
        if (message == null) return NotFound();
        if (message.AuthorId != user.EmployeeId) return Forbid();
        message.Content = request.Content;
        await _context.SaveChangesAsync();
        return Ok(new { message.Id, message.Content });
    }

    [HttpDelete("channels/{channelId}/messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(int channelId, int messageId)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var message = await _context.ChannelMessages.FirstOrDefaultAsync(m => m.Id == messageId && m.ChannelId == channelId);
        if (message == null) return NotFound();

        var isAdmin = User.IsInRole("Administrator") || User.IsInRole("HRManager");
        if (message.AuthorId != user.EmployeeId && !isAdmin) return Forbid();

        _context.ChannelMessages.Remove(message);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("channels/{id}/messages")]
    public async Task<IActionResult> PostMessage(int id, [FromBody] PostChannelMessageRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var channel = await _context.CollaborationChannels.FindAsync(id);
        if (channel == null) return NotFound();

        var message = new ChannelMessage
        {
            ChannelId = id,
            AuthorId = user.EmployeeId.Value,
            Content = request.Content,
            MessageType = request.MessageType ?? "Message"
        };
        _context.ChannelMessages.Add(message);
        await _context.SaveChangesAsync();

        await _context.Entry(message).Reference(m => m.Author).LoadAsync();
        return Ok(new
        {
            message.Id, message.Content, message.MessageType, message.IsPinned, message.CreatedAt,
            AuthorName = $"{message.Author.FirstName} {message.Author.LastName}",
            AuthorId = message.AuthorId
        });
    }
}

public class CreateChannelRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ChannelType { get; set; }
    public int? DepartmentId { get; set; }
}

public class EditMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

public class PostChannelMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public string? MessageType { get; set; }
}
