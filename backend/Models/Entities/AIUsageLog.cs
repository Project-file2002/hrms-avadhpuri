namespace HRMS.API.Models.Entities;

public class AIUsageLog
{
    public int Id { get; set; }
    public string Feature { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string? Response { get; set; }
    public bool Success { get; set; }
    public int? TokensUsed { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
}
