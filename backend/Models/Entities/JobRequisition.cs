namespace HRMS.API.Models.Entities;

public class JobRequisition
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public ICollection<CandidateProfile> Candidates { get; set; } = new List<CandidateProfile>();
}
