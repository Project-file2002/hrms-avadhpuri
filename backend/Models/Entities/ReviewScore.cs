namespace HRMS.API.Models.Entities;

public class ReviewScore
{
    public int Id { get; set; }
    public string Criteria { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string? Comments { get; set; }

    public int ReviewId { get; set; }
    public PerformanceReview Review { get; set; } = null!;
}
