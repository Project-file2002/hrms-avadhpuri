using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;
using HRMS.API.Models.DTOs.Career;
using HRMS.API.Utils;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/careers/wizard")]
public class CandidateWizardController : ControllerBase
{
    private readonly HRMSDbContext _context;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public CandidateWizardController(HRMSDbContext context) => _context = context;

    // ========== PROFESSIONAL STATUS CONFIG ==========

    [HttpGet("status-config")]
    [AllowAnonymous]
    public IActionResult GetStatusConfig()
    {
        var configs = new[]
        {
            new ProfessionalStatusConfig
            {
                Status = "Student", Label = "Student (Currently Studying)", Icon = "graduation-cap",
                Description = "Currently enrolled in a degree/diploma program",
                ShowSections = new() { "basic", "contact", "education", "projects", "skills", "certifications", "training", "social", "achievements", "languages", "documents", "preferences", "consent" },
                HideSections = new() { "experience", "professional", "references", "work-authorization" }
            },
            new ProfessionalStatusConfig
            {
                Status = "Fresher", Label = "Recent Graduate / Fresher", Icon = "user-graduate",
                Description = "Graduated, looking for first job",
                ShowSections = new() { "basic", "contact", "education", "projects", "internships", "skills", "certifications", "training", "social", "achievements", "languages", "documents", "preferences", "consent" },
                HideSections = new() { "experience", "professional", "references", "work-authorization" }
            },
            new ProfessionalStatusConfig
            {
                Status = "Working", Label = "Working Professional", Icon = "briefcase",
                Description = "Currently employed full-time",
                ShowSections = new() { "basic", "contact", "professional", "education", "experience", "projects", "skills", "certifications", "social", "languages", "documents", "references", "work-authorization", "preferences", "consent" },
                HideSections = new() { }
            },
            new ProfessionalStatusConfig
            {
                Status = "ServingNotice", Label = "Serving Notice Period", Icon = "clock",
                Description = "Currently serving notice at current employer",
                ShowSections = new() { "basic", "contact", "professional", "education", "experience", "projects", "skills", "certifications", "social", "languages", "documents", "references", "work-authorization", "preferences", "consent" },
                HideSections = new() { }
            },
            new ProfessionalStatusConfig
            {
                Status = "Freelancer", Label = "Freelancer / Consultant", Icon = "laptop-code",
                Description = "Working independently on client projects",
                ShowSections = new() { "basic", "contact", "professional", "projects", "skills", "certifications", "social", "languages", "documents", "references", "work-authorization", "preferences", "consent" },
                HideSections = new() { "experience" }
            },
            new ProfessionalStatusConfig
            {
                Status = "CareerBreak", Label = "Career Break", Icon = "pause-circle",
                Description = "Took a break from professional work",
                ShowSections = new() { "basic", "contact", "education", "projects", "skills", "certifications", "training", "social", "languages", "documents", "preferences", "consent" },
                HideSections = new() { "experience", "professional", "references", "work-authorization" }
            },
            new ProfessionalStatusConfig
            {
                Status = "LookingInternship", Label = "Looking for Internship", Icon = "internship",
                Description = "Seeking internship opportunity (student or recent graduate)",
                ShowSections = new() { "basic", "contact", "education", "projects", "skills", "certifications", "training", "social", "achievements", "languages", "documents", "preferences", "consent" },
                HideSections = new() { "experience", "professional", "references", "work-authorization" }
            },
        };
        return Ok(configs);
    }

    // ========== RESUME UPLOAD ==========

    [HttpPost("upload")]
    [Authorize(Roles = "Candidate")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadResume(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".pdf" && ext != ".doc" && ext != ".docx")
            return BadRequest(new { message = "Only PDF, DOC, DOCX allowed" });

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var url = $"/uploads/resumes/{fileName}";

        var text = ResumeTextExtractor.Extract(filePath);

        return Ok(new { url, fileName = file.FileName, size = file.Length, extractedText = text });
    }

    // ========== AI RESUME PARSE ==========

    [HttpPost("parse-resume")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> ParseResume([FromBody] ParseResumeRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId && c.JobRequisitionId == request.JobId);
        if (profile == null) return NotFound("No draft found. Start an application first.");

        profile.ResumeText = request.Text;
        profile.ResumePath = request.FileUrl;
        profile.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var lines = (request.Text ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var emails = System.Text.RegularExpressions.Regex.Matches(request.Text ?? "", @"[\w.-]+@[\w.-]+\.\w+")
            .Select(m => m.Value).Distinct().ToList();

        return Ok(new
        {
            success = true,
            emails,
            phone = System.Text.RegularExpressions.Regex.Match(request.Text ?? "", @"[\+]?[\d\-\(\)\s]{10,}").Value,
            wordCount = (request.Text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
            linesExtracted = lines.Length,
            message = "Resume text saved successfully"
        });
    }

    // ========== WIZARD CONFIG (sections for given status) ==========

    [HttpGet("config")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWizardConfig([FromQuery] string? status, [FromQuery] int? jobId)
    {
        var sections = GetDefaultSections(status ?? "Working", jobId);
        return Ok(new WizardConfigDto
        {
            Sections = sections,
            TotalSteps = sections.Count,
            CompletedSteps = 0
        });
    }

    // ========== SAVE STEP ==========

    [HttpPost("step")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> SaveStep([FromBody] SaveStepRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.CandidateProfiles).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return Unauthorized();

        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId && c.JobRequisitionId == request.JobId);

        if (profile == null)
        {
            var job = await _context.JobRequisitions.FindAsync(request.JobId);
            if (job == null) return NotFound("Job not found");
            profile = new CandidateProfile
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                JobRequisitionId = request.JobId,
                Status = "Draft",
                Source = "Career Portal",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.CandidateProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        await SaveStepData(profile.Id, request.StepKey, request.Data);
        await HandleStepSideEffects(profile.Id, request.StepKey, request.Data);

        var totalSections = GetDefaultSections(null, request.JobId);
        var completed = await _context.CandidateStepDatas.CountAsync(c => c.ProfileId == profile.Id);
        return Ok(new SaveStepResponse
        {
            Success = true,
            CompletedSteps = completed,
            TotalSteps = totalSections.Count,
            Message = $"Step '{request.StepKey}' saved successfully."
        });
    }

    // ========== LOAD STEP ==========

    [HttpGet("step/{jobId}/{stepKey}")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> LoadStep(int jobId, string stepKey)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId && c.JobRequisitionId == jobId);
        if (profile == null)
            return Ok(new StepDataResponse { StepKey = stepKey, IsCompleted = false, Data = new(), Items = new() });

        var saved = await _context.CandidateStepDatas
            .FirstOrDefaultAsync(s => s.ProfileId == profile.Id && s.StepKey == stepKey);
        if (saved == null)
            return Ok(new StepDataResponse { StepKey = stepKey, IsCompleted = false, Data = new(), Items = new() });

        var data = JsonSerializer.Deserialize<Dictionary<string, object?>>(saved.JsonData, JsonOpts) ?? new();
        var items = new List<Dictionary<string, object?>>();

        if (stepKey is "education" or "experience" or "projects" or "certifications" or "achievements" or "languages" or "references" or "training")
            items = await LoadChildItems(profile.Id, stepKey);

        return Ok(new StepDataResponse { StepKey = stepKey, IsCompleted = true, Data = data, Items = items });
    }

    // ========== PROGRESS ==========

    [HttpGet("progress/{jobId}")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetProgress(int jobId)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId && c.JobRequisitionId == jobId);

        if (profile == null)
            return Ok(new WizardProgressDto { JobId = jobId, TotalSteps = 0 });

        var extended = await _context.CandidateProfileExtendeds
            .FirstOrDefaultAsync(e => e.CandidateProfileId == profile.Id);
        var status = extended?.ProfessionalStatus ?? "Working";

        var completedKeys = await _context.CandidateStepDatas
            .Where(s => s.ProfileId == profile.Id)
            .Select(s => s.StepKey)
            .ToListAsync();

        var sections = GetDefaultSections(status, jobId);
        var stepIdx = -1;
        var steps = sections.Select((s, i) =>
        {
            var isCompleted = completedKeys.Contains(s.Key);
            if (!isCompleted && stepIdx == -1) stepIdx = i;
            return new StepStatusDto
            {
                Key = s.Key,
                Title = s.Title,
                IsCompleted = isCompleted,
                IsCurrent = !isCompleted && stepIdx == -1
            };
        }).ToList();

        return Ok(new WizardProgressDto
        {
            ProfileId = profile.Id,
            JobId = jobId,
            ProfessionalStatus = status,
            CompletedSteps = completedKeys.Count,
            TotalSteps = sections.Count,
            Steps = steps
        });
    }

    // ========== SUBMIT WIZARD ==========

    [HttpPost("submit/{jobId}")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> SubmitWizard(int jobId)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId && c.JobRequisitionId == jobId);

        if (profile == null) return NotFound("No draft found. Please start your application.");
        if (profile.Status != "Draft") return BadRequest("Already submitted.");

        var extended = await _context.CandidateProfileExtendeds
            .FirstOrDefaultAsync(e => e.CandidateProfileId == profile.Id);

        // Sync profile basics from extended
        if (extended != null)
        {
            if (!string.IsNullOrEmpty(extended.CurrentLocation))
                profile.Phone = extended.AlternatePhone ?? profile.Phone;
        }

        profile.Status = "New";
        profile.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { profile.Id, Status = profile.Status, Message = "Application submitted successfully!" });
    }

    // ========== DASHBOARD ==========

    [HttpGet("dashboard")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users
            .Include(u => u.CandidateProfiles!).ThenInclude(c => c.JobRequisition!).ThenInclude(j => j.Department)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return Unauthorized();

        var apps = user.CandidateProfiles?
            .Where(c => c.Status != "Draft")
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ApplicationSummaryDto
            {
                ApplicationId = c.Id,
                JobTitle = c.JobRequisition?.Title ?? "Role",
                Department = c.JobRequisition?.Department?.Name ?? "General",
                Status = c.Status,
                StatusLabel = GetStatusLabel(c.Status),
                MatchScore = c.MatchScore.HasValue ? (int)Math.Round(c.MatchScore.Value) : null,
                AppliedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                NextStepHint = GetNextStepHint(c.Status),
                Pipeline = BuildPipeline(c.Status)
            }).ToList() ?? new();

        return Ok(new CandidateDashboardDto
        {
            ApplicationCount = apps.Count,
            Applications = apps,
            ResumeScore = null, // Loaded client-side via AI
            CareerAdvice = null
        });
    }

    // ========== PRIVATE HELPERS ==========

    private async Task SaveStepData(int profileId, string stepKey, Dictionary<string, object?> data)
    {
        var existing = await _context.CandidateStepDatas
            .FirstOrDefaultAsync(s => s.ProfileId == profileId && s.StepKey == stepKey);
        if (existing != null)
        {
            existing.JsonData = JsonSerializer.Serialize(data, JsonOpts);
            existing.SavedAt = DateTime.UtcNow;
        }
        else
        {
            _context.CandidateStepDatas.Add(new Models.Entities.CandidateStepData
            {
                ProfileId = profileId,
                StepKey = stepKey,
                JsonData = JsonSerializer.Serialize(data, JsonOpts)
            });
        }
        await _context.SaveChangesAsync();
    }

    private async Task HandleStepSideEffects(int profileId, string stepKey, Dictionary<string, object?> data)
    {
        if (stepKey == "professional-status" && data.TryGetValue("status", out var statusObj))
        {
            var status = statusObj?.ToString();
            var profile = await _context.CandidateProfiles.FindAsync(profileId);
            if (profile != null)
            {
                var ext = await _context.CandidateProfileExtendeds
                    .FirstOrDefaultAsync(e => e.CandidateProfileId == profileId);
                if (ext == null)
                {
                    ext = new CandidateProfileExtended { CandidateProfileId = profileId };
                    _context.CandidateProfileExtendeds.Add(ext);
                }
                ext.ProfessionalStatus = status;
                ext.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        if (stepKey == "basic" && data.TryGetValue("firstName", out var fn) && data.TryGetValue("lastName", out var ln))
        {
            var profile = await _context.CandidateProfiles.FindAsync(profileId);
            if (profile != null)
            {
                if (fn?.ToString() is { Length: > 0 }) profile.FirstName = fn.ToString()!;
                if (ln?.ToString() is { Length: > 0 }) profile.LastName = ln.ToString()!;
                if (data.TryGetValue("email", out var email) && email?.ToString() is { Length: > 0 }) profile.Email = email.ToString()!;
                if (data.TryGetValue("phone", out var phone) && phone?.ToString() is { Length: > 0 }) profile.Phone = phone.ToString()!;
                profile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // For multi-entry steps, save as child entities
        if (stepKey == "basic" || stepKey == "contact" || stepKey == "professional" ||
            stepKey == "preferences" || stepKey == "work-authorization" || stepKey == "consent" ||
            stepKey == "emergency" || stepKey == "availability" || stepKey == "diversity")
        {
            await SaveToExtended(profileId, data);
        }
    }

    private async Task SaveToExtended(int profileId, Dictionary<string, object?> data)
    {
        var ext = await _context.CandidateProfileExtendeds.FirstOrDefaultAsync(e => e.CandidateProfileId == profileId);
        if (ext == null)
        {
            ext = new CandidateProfileExtended { CandidateProfileId = profileId };
            _context.CandidateProfileExtendeds.Add(ext);
        }

        // Map data to extended properties
        var type = typeof(CandidateProfileExtended);
        foreach (var kv in data)
        {
            var prop = type.GetProperty(kv.Key);
            if (prop != null && prop.CanWrite)
            {
                var val = kv.Value;
                if (val is JsonElement je)
                {
                    if (je.ValueKind == JsonValueKind.String) prop.SetValue(ext, je.GetString());
                    else if (je.ValueKind == JsonValueKind.Number) prop.SetValue(ext, je.GetDecimal());
                    else if (je.ValueKind == JsonValueKind.True || je.ValueKind == JsonValueKind.False) prop.SetValue(ext, je.GetBoolean());
                    else if (je.ValueKind == JsonValueKind.Null) prop.SetValue(ext, null);
                }
                else
                {
                    prop.SetValue(ext, val);
                }
            }
        }

        ext.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task<List<Dictionary<string, object?>>> LoadChildItems(int profileId, string stepKey)
    {
        var result = new List<Dictionary<string, object?>>();
        IQueryable<object>? query = stepKey switch
        {
            "education" => _context.CandidateEducations.Where(e => e.CandidateProfileId == profileId),
            "experience" => _context.CandidateExperiences.Where(e => e.CandidateProfileId == profileId),
            "projects" => _context.CandidateProjects.Where(e => e.CandidateProfileId == profileId),
            "certifications" => _context.CandidateCertifications.Where(e => e.CandidateProfileId == profileId),
            "achievements" => _context.CandidateAchievements.Where(e => e.CandidateProfileId == profileId),
            "languages" => _context.CandidateLanguages.Where(e => e.CandidateProfileId == profileId),
            "references" => _context.CandidateReferences.Where(e => e.CandidateProfileId == profileId),
            "training" => _context.CandidateTrainings.Where(e => e.CandidateProfileId == profileId),
            _ => null
        };

        if (query != null)
        {
            var items = await query.Cast<object>().ToListAsync();
            foreach (var item in items)
                result.Add(JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(item, JsonOpts), JsonOpts) ?? new());
        }
        return result;
    }

    private static List<WizardSectionDto> GetDefaultSections(string? status, int? jobId)
    {
        var all = new List<WizardSectionDto>
        {
            new() { Key = "professional-status", Title = "Professional Status", StepNumber = 1, IsRequired = true },
            new() { Key = "basic", Title = "Basic Information", StepNumber = 2, IsRequired = true },
            new() { Key = "contact", Title = "Contact & Social Profiles", StepNumber = 3, IsRequired = true },
            new() { Key = "professional", Title = "Professional Info", StepNumber = 4, IsRequired = true },
            new() { Key = "education", Title = "Education", StepNumber = 5, IsRequired = true },
            new() { Key = "experience", Title = "Work Experience", StepNumber = 6, IsRequired = false },
            new() { Key = "projects", Title = "Projects", StepNumber = 7, IsRequired = true },
            new() { Key = "internships", Title = "Internships", StepNumber = 8, IsRequired = false },
            new() { Key = "skills", Title = "Skills & Expertise", StepNumber = 9, IsRequired = true },
            new() { Key = "certifications", Title = "Certifications", StepNumber = 10, IsRequired = false },
            new() { Key = "training", Title = "Training & Courses", StepNumber = 11, IsRequired = false },
            new() { Key = "social", Title = "Coding Profiles", StepNumber = 12, IsRequired = false },
            new() { Key = "achievements", Title = "Achievements", StepNumber = 13, IsRequired = false },
            new() { Key = "languages", Title = "Languages", StepNumber = 14, IsRequired = false },
            new() { Key = "documents", Title = "Resume & Documents", StepNumber = 15, IsRequired = true },
            new() { Key = "references", Title = "References", StepNumber = 16, IsRequired = false },
            new() { Key = "work-authorization", Title = "Work Authorization", StepNumber = 17, IsRequired = false },
            new() { Key = "diversity", Title = "Diversity (Optional)", StepNumber = 18, IsRequired = false },
            new() { Key = "emergency", Title = "Emergency Contact", StepNumber = 19, IsRequired = false },
            new() { Key = "availability", Title = "Availability", StepNumber = 20, IsRequired = false },
            new() { Key = "preferences", Title = "Job Preferences", StepNumber = 21, IsRequired = true },
            new() { Key = "ai-section", Title = "AI Analysis", StepNumber = 22, IsRequired = false },
            new() { Key = "questions", Title = "Job Questions", StepNumber = 23, IsRequired = false },
            new() { Key = "consent", Title = "Review & Submit", StepNumber = 24, IsRequired = true },
        };

        if (string.IsNullOrEmpty(status)) return all;

        var config = status switch
        {
            "Student" => new[] { "professional", "experience", "internships", "references", "work-authorization" },
            "Fresher" => new[] { "professional", "experience", "references", "work-authorization" },
            "Freelancer" => new[] { "experience" },
            "CareerBreak" => new[] { "professional", "experience", "references", "work-authorization" },
            "LookingInternship" => new[] { "professional", "experience", "references", "work-authorization" },
            _ => Array.Empty<string>()
        };

        return all.Where(s => !config.Contains(s.Key)).ToList();
    }

    private static string GetStatusLabel(string status) => status switch
    {
        "Draft" => "Draft", "New" => "Applied", "Screening" => "AI Screening",
        "Interviewed" => "Interview", "Offered" => "Offer", "Hired" => "Hired",
        "Rejected" => "Rejected", "Withdrawn" => "Withdrawn", _ => status
    };

    private static string GetNextStepHint(string status) => status switch
    {
        "New" => "AI screening in progress",
        "Screening" => "HR review typically takes 2-3 days",
        "Interviewed" => "Awaiting interview feedback",
        "Offered" => "Offer letter is being prepared",
        _ => ""
    };

    private static List<PipelineStageDto> BuildPipeline(string status)
    {
        var stages = new[] {
            ("Applied", "Application submitted"),
            ("AI Screening", "Resume matched against job description"),
            ("HR Review", "Recruiter validates your profile"),
            ("Interview", "Technical and manager rounds"),
            ("Offer", "Offer letter and negotiation"),
            ("Hired", "Welcome to the team")
        };

        var currentIndex = status switch
        {
            "New" => 0, "Screening" => 2, "Interviewed" => 3,
            "Offered" => 4, "Hired" => 5, "Rejected" => -1, _ => 1
        };

        return stages.Select((s, i) => new PipelineStageDto
        {
            Title = s.Item1,
            Description = s.Item2,
            Completed = currentIndex >= 0 && i < currentIndex,
            Current = currentIndex >= 0 && i == currentIndex
        }).ToList();
    }
}
