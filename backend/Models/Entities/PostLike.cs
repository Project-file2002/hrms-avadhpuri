namespace HRMS.API.Models.Entities;

public class PostLike
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public SocialPost Post { get; set; } = null!;
    public int UserId { get; set; }
}
