namespace HRMS.API.Models.Entities;

public class PerformanceReview
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Comments { get; set; }
    public decimal? OverallScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int ReviewerId { get; set; }
    public Employee Reviewer { get; set; } = null!;
    public int CycleId { get; set; }
    public ReviewCycle Cycle { get; set; } = null!;
    public ICollection<ReviewScore> Scores { get; set; } = new List<ReviewScore>();
}
