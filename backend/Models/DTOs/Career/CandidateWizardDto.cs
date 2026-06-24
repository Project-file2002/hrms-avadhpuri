using HRMS.API.Models.Entities;

namespace HRMS.API.Models.DTOs.Career;

// ========== WIZARD CONFIG ==========

public class WizardConfigDto
{
    public List<WizardSectionDto> Sections { get; set; } = new();
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
}

public class WizardSectionDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StepNumber { get; set; }
    public bool IsRequired { get; set; }
    public bool IsCompleted { get; set; }
    public List<WizardFieldDto> Fields { get; set; } = new();
}

public class WizardFieldDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public bool IsRequired { get; set; }
    public string? Placeholder { get; set; }
    public string? DefaultValue { get; set; }
    public string[]? Options { get; set; }
    public string? HelpText { get; set; }
    public bool Hidden { get; set; }
}

// ========== SAVE STEP ==========

public class SaveStepRequest
{
    public int JobId { get; set; }
    public string StepKey { get; set; } = string.Empty;
    public Dictionary<string, object?> Data { get; set; } = new();
}

public class SaveStepResponse
{
    public bool Success { get; set; }
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public string? Message { get; set; }
}

// ========== PROFESSIONAL STATUS ==========

public class ProfessionalStatusConfig
{
    public string Status { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> ShowSections { get; set; } = new();
    public List<string> HideSections { get; set; } = new();
}

// ========== STEP DATA (for returning saved step) ==========

public class StepDataResponse
{
    public string StepKey { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public Dictionary<string, object?> Data { get; set; } = new();
    public List<Dictionary<string, object?>> Items { get; set; } = new(); // for multi-entry steps
}

// ========== WIZARD PROGRESS ==========

public class WizardProgressDto
{
    public int ProfileId { get; set; }
    public int JobId { get; set; }
    public string ProfessionalStatus { get; set; } = string.Empty;
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public List<StepStatusDto> Steps { get; set; } = new();
}

public class StepStatusDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
}

// ========== AI SECTION RESPONSES ==========

public class AiResumeScoreDto
{
    public int ResumeScore { get; set; }
    public int AtsScore { get; set; }
    public List<string> MissingSkills { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public string? AiAnalysis { get; set; }
}

public class AiCareerAdviceDto
{
    public List<CareerOptionDto> RecommendedRoles { get; set; } = new();
    public List<SkillGapDto> SkillGaps { get; set; } = new();
    public LearningRoadmapDto? LearningRoadmap { get; set; }
    public InterviewReadinessDto? InterviewReadiness { get; set; }
}

public class CareerOptionDto
{
    public string Role { get; set; } = string.Empty;
    public int MatchPercent { get; set; }
    public string? Reason { get; set; }
}

public class SkillGapDto
{
    public string Skill { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public int RequiredLevel { get; set; }
    public int Gap { get; set; }
}

public class LearningRoadmapDto
{
    public string CurrentSkills { get; set; } = string.Empty;
    public string TargetJob { get; set; } = string.Empty;
    public List<string> MissingSkills { get; set; } = new();
    public List<string> Steps { get; set; } = new();
    public List<string> Resources { get; set; } = new();
}

public class InterviewReadinessDto
{
    public int Technical { get; set; }
    public int Behavioral { get; set; }
    public int Communication { get; set; }
}

// ========== CANDIDATE DASHBOARD ==========

public class CandidateDashboardDto
{
    public int ProfileId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfessionalStatus { get; set; }
    public int ApplicationCount { get; set; }
    public List<ApplicationSummaryDto> Applications { get; set; } = new();
    public AiResumeScoreDto? ResumeScore { get; set; }
    public AiCareerAdviceDto? CareerAdvice { get; set; }
}

public class ApplicationSummaryDto
{
    public int ApplicationId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public int? MatchScore { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? NextStepHint { get; set; }
    public List<PipelineStageDto> Pipeline { get; set; } = new();
}

public class PipelineStageDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public bool Current { get; set; }
}

// ========== WIZARD QUERY ==========

public class WizardQueryRequest
{
    public int JobId { get; set; }
}

public class CandidateStepData
{
    public int ProfileId { get; set; }
    public string StepKey { get; set; } = string.Empty;
    public string JsonData { get; set; } = "{}";
    public DateTime SavedAt { get; set; }
}

public class ParseResumeRequest
{
    public int JobId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
}
