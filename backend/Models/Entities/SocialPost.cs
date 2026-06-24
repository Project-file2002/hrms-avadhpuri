namespace HRMS.API.Models.Entities;

public class SocialPost
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "Post"; // Post, Recognition, Announcement
    public string? ImageUrl { get; set; }
    public string? Tags { get; set; } // JSON array of tags
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int AuthorId { get; set; }
    public Employee Author { get; set; } = null!;

    public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
}
