namespace HRMS.API.Models.DTOs.Career;

public class CareerPortalConfigDto
{
    public string CompanyName { get; set; } = "EWXP Technologies";
    public string Tagline { get; set; } = "Build the future of work — one team, one company.";
    public bool IsMultiCompany { get; set; } = false;
}

public class CareerJobListingDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = "Pune";
    public string Workplace { get; set; } = "Hybrid";
    public string JobType { get; set; } = "Full Time";
    public string Experience { get; set; } = "3-5 Years";
    public string SalaryRange { get; set; } = "8-12 LPA";
    public List<string> Skills { get; set; } = new();
    public bool Featured { get; set; }
    public bool IsRemote { get; set; }
    public int DaysOpen { get; set; }
    public DateTime PostedAt { get; set; }
}

public class CareerMatchRequest
{
    public int JobId { get; set; }
    public string? ResumeText { get; set; }
    public string? SkillsText { get; set; }
}

public class CareerSkillMatchDto
{
    public string Skill { get; set; } = string.Empty;
    public bool Matched { get; set; }
}

public class CareerMatchResponse
{
    public int MatchPercent { get; set; }
    public List<CareerSkillMatchDto> Skills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
    public List<string> StrongAreas { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class CareerExplainJobRequest
{
    public int JobId { get; set; }
    public string? ResumeText { get; set; }
}

public class CareerExplainJobResponse
{
    public string Summary { get; set; } = string.Empty;
    public int ProfileMatchPercent { get; set; }
    public List<string> MissingSkills { get; set; } = new();
    public List<string> RecommendedLearning { get; set; } = new();
    public string InterviewDifficulty { get; set; } = "Medium";
}

public class CareerResumeReviewRequest
{
    public int? JobId { get; set; }
    public string ResumeText { get; set; } = string.Empty;
}

public class CareerResumeReviewResponse
{
    public int AtsScore { get; set; }
    public string Grammar { get; set; } = "Good";
    public List<string> MissingKeywords { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public string? AiAnalysis { get; set; }
}

public class CareerSearchRequest
{
    public string Query { get; set; } = string.Empty;
}

public class CareerSearchResponse
{
    public string InterpretedAs { get; set; } = string.Empty;
    public List<int> JobIds { get; set; } = new();
}

public class CareerAssistantRequest
{
    public string Message { get; set; } = string.Empty;
}

public class CareerAssistantResponse
{
    public string Reply { get; set; } = string.Empty;
    public List<int> JobIds { get; set; } = new();
    public Dictionary<string, string> AppliedFilters { get; set; } = new();
}

public class CareerApplyRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int JobRequisitionId { get; set; }
    public string? ResumeText { get; set; }
    public string? SkillsText { get; set; }
}

public class CareerRegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class CareerLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CareerApplyResponse
{
    public int ApplicationId { get; set; }
    public string Status { get; set; } = "Screening";
    public int MatchScore { get; set; }
    public int ShortlistProbability { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CandidateProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
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
}

public class UpdateProfileRequest
{
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
}

public class CareerApplicationStageDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public bool Current { get; set; }
}

public class CareerApplicationDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public int? MatchScore { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? NextStepHint { get; set; }
    public List<CareerApplicationStageDto> Pipeline { get; set; } = new();
}
