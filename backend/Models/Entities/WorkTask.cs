namespace HRMS.API.Models.Entities;

public class WorkTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Pending";
    public string Priority { get; set; } = "Normal";
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public int AssignedToId { get; set; }
    public Employee AssignedTo { get; set; } = null!;

    public int? AssignedById { get; set; }
    public Employee? AssignedBy { get; set; }

    public int? MeetingId { get; set; }
    public Meeting? Meeting { get; set; }
}
