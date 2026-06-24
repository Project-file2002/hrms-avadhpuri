namespace HRMS.API.Models.Entities;

public class DiscussionThread
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string? Tags { get; set; }
    public bool IsPinned { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int CreatedById { get; set; }
    public Employee CreatedBy { get; set; } = null!;

    public ICollection<DiscussionReply> Replies { get; set; } = new List<DiscussionReply>();
}

public class DiscussionReply
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int ThreadId { get; set; }
    public DiscussionThread Thread { get; set; } = null!;

    public int CreatedById { get; set; }
    public Employee CreatedBy { get; set; } = null!;
}
