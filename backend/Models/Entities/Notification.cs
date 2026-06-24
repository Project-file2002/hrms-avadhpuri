namespace HRMS.API.Models.Entities;

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public string Category { get; set; } = "Information";
    public string Source { get; set; } = "System";
    public string Priority { get; set; } = "Normal";
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public bool IsStarred { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
