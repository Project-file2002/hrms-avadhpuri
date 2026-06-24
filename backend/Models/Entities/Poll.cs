namespace HRMS.API.Models.Entities;

public class Poll
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public bool MultiVote { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public int CreatedById { get; set; }
    public Employee CreatedBy { get; set; } = null!;

    public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
}

public class PollOption
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;

    public int PollId { get; set; }
    public Poll Poll { get; set; } = null!;

    public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();
}

public class PollVote
{
    public int Id { get; set; }

    public int PollOptionId { get; set; }
    public PollOption PollOption { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
}
