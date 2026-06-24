namespace HRMS.API.Models.Entities;

public class CandidateMasterProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? CurrentAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public string? ProfessionalStatus { get; set; }
    public string? CurrentCompany { get; set; }
    public string? CurrentDesignation { get; set; }
    public int? TotalExperienceMonths { get; set; }
    public decimal? CurrentCtc { get; set; }
    public decimal? ExpectedCtc { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? ResumePath { get; set; }
    public string? ResumeFileName { get; set; }
    public string? ResumeText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
