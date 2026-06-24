namespace HRMS.API.Models.DTOs.Performance;

public class PerformanceReviewDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public decimal? OverallScore { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public int CycleId { get; set; }
    public string CycleName { get; set; } = string.Empty;
    public List<ReviewScoreDto> Scores { get; set; } = new();
}

public class UpdatePerformanceReview
{
    public string? Title { get; set; }
    public string? Comments { get; set; }
    public string? Status { get; set; }
    public int? CycleId { get; set; }
    public List<UpdateReviewScore>? Scores { get; set; }
}

public class UpdateReviewScore
{
    public string Criteria { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string? Comments { get; set; }
}

public class ReviewScoreDto
{
    public string Criteria { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string? Comments { get; set; }
}

public class CreatePerformanceReview
{
    public string Title { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public int CycleId { get; set; }
}

public class ReviewCycleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
