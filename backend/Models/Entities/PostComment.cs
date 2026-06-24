namespace HRMS.API.Models.Entities;

public class PostComment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int PostId { get; set; }
    public SocialPost Post { get; set; } = null!;
    public int AuthorId { get; set; }
    public Employee Author { get; set; } = null!;
}
