using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SocialController : ControllerBase
{
    private readonly HRMSDbContext _context;

    public SocialController(HRMSDbContext context) => _context = context;

    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var posts = await _context.SocialPosts
            .Include(p => p.Author)
            .Include(p => p.Likes)
            .Include(p => p.Comments).ThenInclude(c => c.Author)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(posts.Select(p => new
        {
            p.Id,
            p.Content,
            p.Type,
            p.ImageUrl,
            p.Tags,
            p.CreatedAt,
            Author = $"{p.Author.FirstName} {p.Author.LastName}",
            AuthorAvatar = p.Author.FirstName[..1],
            LikeCount = p.Likes.Count,
            IsLiked = false,
            Comments = p.Comments.OrderBy(c => c.CreatedAt).Select(c => new
            {
                c.Id,
                c.Content,
                c.CreatedAt,
                Author = $"{c.Author.FirstName} {c.Author.LastName}"
            })
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSocialPost request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.Employee == null) return Unauthorized();

        var post = new SocialPost
        {
            Content = request.Content,
            Type = request.Type ?? "Post",
            ImageUrl = request.ImageUrl,
            Tags = request.Tags,
            AuthorId = user.Employee.Id
        };
        _context.SocialPosts.Add(post);
        await _context.SaveChangesAsync();
        return Ok(post);
    }

    [HttpPost("{id}/like")]
    public async Task<IActionResult> ToggleLike(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var existing = await _context.PostLikes.FirstOrDefaultAsync(pl => pl.PostId == id && pl.UserId == userId);
        if (existing != null)
        {
            _context.PostLikes.Remove(existing);
        }
        else
        {
            _context.PostLikes.Add(new PostLike { PostId = id, UserId = userId });
        }
        await _context.SaveChangesAsync();
        return Ok(new { liked = existing == null });
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.Employee == null) return Unauthorized();

        var comment = new PostComment
        {
            PostId = id,
            Content = request.Content,
            AuthorId = user.Employee.Id
        };
        _context.PostComments.Add(comment);
        await _context.SaveChangesAsync();
        return Ok(new { comment.Id, comment.Content, comment.CreatedAt, Author = $"{user.Employee.FirstName} {user.Employee.LastName}" });
    }

    [Authorize(Roles = "Administrator,HRManager")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _context.SocialPosts.FindAsync(id);
        if (post == null) return NotFound();
        _context.SocialPosts.Remove(post);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class CreateSocialPost
{
    public string Content { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? ImageUrl { get; set; }
    public string? Tags { get; set; }
}

public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
}
