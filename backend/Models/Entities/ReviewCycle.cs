namespace HRMS.API.Models.Entities;

public class ReviewCycle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public ICollection<PerformanceReview> Reviews { get; set; } = new List<PerformanceReview>();
}
