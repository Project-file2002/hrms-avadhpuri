using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Auth;
using HRMS.API.Models.DTOs.Career;
using HRMS.API.Models.Entities;
using HRMS.API.Models.Mapping;
using HRMS.API.Services.Interfaces;
using HRMS.API.Utils;

namespace HRMS.API.Services;

public class CareerPortalService : ICareerPortalService
{
    private readonly HRMSDbContext _context;
    private readonly IAIService _aiService;
    private readonly IConfiguration _configuration;
    private readonly IGroqResumeParser _groqParser;

    private static readonly Dictionary<string, string[]> SkillAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["java"] = ["java", "spring", "spring boot", "hibernate", "microservices", "backend"],
        ["react"] = ["react", "reactjs", "frontend", "typescript", "javascript", "redux", "next.js", "nextjs"],
        ["python"] = ["python", "django", "flask", "data science", "ml"],
        ["devops"] = ["docker", "kubernetes", "k8s", "terraform", "ci/cd", "devops", "aws", "azure"],
        ["sales"] = ["sales", "crm", "salesforce", "analytics", "excel", "sql"],
        [".net"] = ["c#", "csharp", ".net", "dotnet", "asp.net", "entity framework"],
    };

    public CareerPortalService(HRMSDbContext context, IAIService aiService, IConfiguration configuration, IGroqResumeParser groqParser)
    {
        _context = context;
        _aiService = aiService;
        _configuration = configuration;
        _groqParser = groqParser;
    }

    public async Task<CareerPortalConfigDto> GetConfigAsync()
    {
        var name = await _context.SystemSettings
            .Where(s => s.Key == "CompanyName")
            .Select(s => s.Value)
            .FirstOrDefaultAsync();

        return new CareerPortalConfigDto
        {
            CompanyName = name ?? "EWXP Technologies",
            Tagline = "AI-powered careers — built for our team, not a job board.",
            IsMultiCompany = false
        };
    }

    public async Task<List<CareerJobListingDto>> GetJobsAsync()
    {
        var jobs = await _context.JobRequisitions
            .Include(j => j.Department)
            .Where(j => j.Status == "Open")
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return jobs.Select(EnrichJob).ToList();
    }

    public async Task<CareerJobListingDto?> GetJobAsync(int id)
    {
        var job = await _context.JobRequisitions
            .Include(j => j.Department)
            .FirstOrDefaultAsync(j => j.Id == id && j.Status == "Open");
        return job == null ? null : EnrichJob(job);
    }

    public async Task<CareerMatchResponse> ComputeMatchAsync(CareerMatchRequest request)
    {
        var job = await GetJobAsync(request.JobId);
        if (job == null)
            return new CareerMatchResponse { Summary = "Job not found." };

        return BuildMatch(job, CombineText(request.ResumeText, request.SkillsText));
    }

    public async Task<CareerExplainJobResponse> ExplainJobAsync(CareerExplainJobRequest request)
    {
        var job = await GetJobAsync(request.JobId);
        if (job == null)
            return new CareerExplainJobResponse { Summary = "Job not found." };

        var match = BuildMatch(job, request.ResumeText ?? "");
        var roleFocus = job.Title.Contains("Frontend", StringComparison.OrdinalIgnoreCase) ? "frontend development"
            : job.Title.Contains("Backend", StringComparison.OrdinalIgnoreCase) ? "backend and microservices"
            : job.Title.Contains("DevOps", StringComparison.OrdinalIgnoreCase) ? "cloud infrastructure and CI/CD"
            : job.Title.Contains("Sales", StringComparison.OrdinalIgnoreCase) ? "sales operations and analytics"
            : "this specialized role";

        return new CareerExplainJobResponse
        {
            Summary = $"This role is mainly focused on **{roleFocus}** at {job.Department}. " +
                      $"You'll work on {job.Description?.TrimEnd('.') ?? "key business initiatives"}. " +
                      $"Work mode: {job.Workplace} · Location: {job.Location}.",
            ProfileMatchPercent = match.MatchPercent,
            MissingSkills = match.MissingSkills.Take(4).ToList(),
            RecommendedLearning = match.MissingSkills.Take(3)
                .Select(s => $"{s} fundamentals course")
                .ToList(),
            InterviewDifficulty = match.MatchPercent >= 85 ? "Easy-Medium" : match.MatchPercent >= 70 ? "Medium" : "Medium-Hard"
        };
    }

    public async Task<CareerResumeReviewResponse> ReviewResumeAsync(CareerResumeReviewRequest request)
    {
        CareerJobListingDto? job = null;
        if (request.JobId.HasValue)
            job = await GetJobAsync(request.JobId.Value);

        var text = request.ResumeText.ToLowerInvariant();
        var keywords = job?.Skills ?? ExtractSkillsFromText(job?.Requirements ?? job?.Title ?? "");
        var found = keywords.Where(k => text.Contains(k.ToLowerInvariant())).ToList();
        var missing = keywords.Where(k => !text.Contains(k.ToLowerInvariant())).Take(5).ToList();

        var ats = keywords.Count == 0 ? 72 : (int)Math.Min(98, 55 + (found.Count * 100.0 / keywords.Count) * 0.45 + Math.Min(text.Length / 40.0, 25));

        var suggestions = new List<string>();
        if (text.Length < 200) suggestions.Add("Add a stronger professional summary (3-4 lines)");
        if (!text.Contains("project")) suggestions.Add("Add 2-3 project highlights with measurable impact");
        if (missing.Count > 0) suggestions.Add($"Include keywords: {string.Join(", ", missing.Take(3))}");
        if (!text.Contains("experience")) suggestions.Add("Structure experience with role, company, and outcomes");

        string? aiAnalysis = null;
        if (job != null && !string.IsNullOrWhiteSpace(request.ResumeText) && request.ResumeText.Length > 80)
        {
            try
            {
                var jd = $"{job.Title}\n{job.Requirements}\nSkills: {string.Join(", ", job.Skills)}";
                aiAnalysis = await _aiService.ScreenCandidateAsync(jd, request.ResumeText);
            }
            catch
            {
                // rule-based fallback only
            }
        }

        return new CareerResumeReviewResponse
        {
            AtsScore = ats,
            Grammar = text.Length > 100 ? "Good" : "Needs improvement",
            MissingKeywords = missing,
            Suggestions = suggestions,
            AiAnalysis = aiAnalysis
        };
    }

    public async Task<CareerSearchResponse> SearchJobsAsync(CareerSearchRequest request)
    {
        var jobs = await GetJobsAsync();
        var (filtered, interpreted) = FilterByNaturalLanguage(jobs, request.Query);
        return new CareerSearchResponse
        {
            InterpretedAs = interpreted,
            JobIds = filtered.Select(j => j.Id).ToList()
        };
    }

    public async Task<CareerAssistantResponse> AssistantQueryAsync(CareerAssistantRequest request)
    {
        var jobs = await GetJobsAsync();
        var (filtered, interpreted) = FilterByNaturalLanguage(jobs, request.Message);
        var filters = ParseFilterLabels(request.Message);

        var reply = filtered.Count == 0
            ? "I couldn't find matching openings right now. Try broadening your search or check back soon."
            : $"Found **{filtered.Count}** relevant opening(s) for you. {interpreted}";

        return new CareerAssistantResponse
        {
            Reply = reply,
            JobIds = filtered.Select(j => j.Id).ToList(),
            AppliedFilters = filters
        };
    }

    private static CareerJobListingDto EnrichJob(JobRequisition job)
    {
        var title = job.Title.ToLowerInvariant();
        var dept = job.Department?.Name ?? "General";
        var meta = ResolveJobMeta(title, dept);
        var daysOpen = (int)(DateTime.UtcNow - job.CreatedAt).TotalDays;

        return new CareerJobListingDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Requirements = job.Requirements,
            Department = dept,
            Location = meta.Location,
            Workplace = meta.Workplace,
            JobType = meta.JobType,
            Experience = meta.Experience,
            SalaryRange = meta.SalaryRange,
            Skills = meta.Skills.ToList(),
            Featured = meta.Featured,
            IsRemote = meta.Workplace is "Remote" or "Hybrid",
            DaysOpen = Math.Max(daysOpen, 1),
            PostedAt = job.CreatedAt
        };
    }

    private static (string Location, string Workplace, string JobType, string Experience, string SalaryRange, string[] Skills, bool Featured) ResolveJobMeta(string title, string dept)
    {
        if (title.Contains("intern"))
            return ("Pune", "Hybrid", "Internship", "0-1 Years", "3-5 LPA", new[] { "JavaScript", "Git", "Problem Solving", "Teamwork" }, true);

        if (title.Contains("react"))
            return ("Pune", "Remote", "Full Time", "3-5 Years", "12-18 LPA", new[] { "React", "TypeScript", "Redux", "Next.js" }, true);

        if (title.Contains("frontend"))
            return ("Pune", "Remote", "Full Time", "3-5 Years", "8-12 LPA", new[] { "React", "TypeScript", "CSS", "REST" }, true);

        if (title.Contains("backend") || title.Contains("java"))
            return ("Pune", "Hybrid", "Full Time", "5-8 Years", "18-25 LPA", new[] { "C#", "Java", "SQL", "Microservices", "Azure" }, true);

        if (title.Contains("devops"))
            return ("Pune", "Hybrid", "Full Time", "5-8 Years", "12-18 LPA", new[] { "Docker", "Kubernetes", "AWS", "Terraform" }, true);

        if (title.Contains("qa") || title.Contains("automation"))
            return ("Pune", "Office", "Full Time", "3-5 Years", "8-12 LPA", new[] { "Selenium", "Cypress", "API Testing", "SQL" }, false);

        if (title.Contains("data"))
            return ("Pune", "Hybrid", "Full Time", "2-4 Years", "10-15 LPA", new[] { "Python", "SQL", "Machine Learning", "Analytics" }, true);

        if (title.Contains("flutter") || title.Contains("mobile"))
            return ("Pune", "Hybrid", "Full Time", "3-5 Years", "10-14 LPA", new[] { "Flutter", "Dart", "REST", "Firebase" }, false);

        if (title.Contains("sales"))
            return ("Mumbai", "Office", "Full Time", "2-5 Years", "6-15 LPA", new[] { "Salesforce", "Excel", "SQL", "CRM" }, false);

        if (title.Contains("marketing") || title.Contains("content"))
            return ("Bangalore", "Remote", "Full Time", "2-5 Years", "6-12 LPA", new[] { "SEO", "Content", "Analytics", "Social Media" }, false);

        if (title.Contains("talent") || title.Contains("hr") || title.Contains("recruit"))
            return ("Pune", "Hybrid", "Full Time", "3-6 Years", "8-14 LPA", new[] { "Recruiting", "HRMS", "Stakeholder Mgmt", "ATS" }, false);

        if (title.Contains("financial") || dept.Contains("Finance"))
            return ("Pune", "Office", "Full Time", "2-4 Years", "7-11 LPA", new[] { "Excel", "Financial Modeling", "SQL", "Reporting" }, false);

        return ("Pune", "Hybrid", "Full Time", "3-5 Years", "6-10 LPA", new[] { "Communication", "Teamwork", "Problem Solving" }, false);
    }

    private static CareerMatchResponse BuildMatch(CareerJobListingDto job, string candidateText)
    {
        var text = candidateText.ToLowerInvariant();
        var jobText = $"{job.Title} {job.Requirements} {string.Join(" ", job.Skills)}".ToLowerInvariant();

        var skillResults = job.Skills.Select(skill => new CareerSkillMatchDto
        {
            Skill = skill,
            Matched = text.Contains(skill.ToLowerInvariant()) || HasAliasMatch(text, skill)
        }).ToList();

        var matched = skillResults.Count(s => s.Matched);
        var total = Math.Max(skillResults.Count, 1);
        var baseScore = (int)Math.Round(matched * 100.0 / total);
        var bonus = Math.Min(15, CountAliasHits(text, jobText) * 3);
        var score = Math.Min(98, Math.Max(42, baseScore + bonus));

        var missing = skillResults.Where(s => !s.Matched).Select(s => s.Skill).ToList();
        var strong = skillResults.Where(s => s.Matched).Select(s => s.Skill).ToList();

        return new CareerMatchResponse
        {
            MatchPercent = score,
            Skills = skillResults,
            MissingSkills = missing,
            StrongAreas = strong,
            Summary = score >= 80
                ? "Strong fit — your profile aligns well with this role."
                : score >= 65
                    ? "Good fit — a few skill gaps can be addressed before applying."
                    : "Moderate fit — consider upskilling on missing areas."
        };
    }

    private static bool HasAliasMatch(string text, string skill)
    {
        foreach (var pair in SkillAliases)
        {
            if (pair.Value.Any(a => a.Equals(skill, StringComparison.OrdinalIgnoreCase) || text.Contains(a)))
                return pair.Value.Any(a => text.Contains(a));
        }
        return false;
    }

    private static int CountAliasHits(string candidateText, string jobText)
    {
        return SkillAliases.Count(pair =>
            pair.Value.Any(v => candidateText.Contains(v)) &&
            pair.Value.Any(v => jobText.Contains(v)));
    }

    private static (List<CareerJobListingDto> jobs, string interpreted) FilterByNaturalLanguage(
        List<CareerJobListingDto> jobs, string query)
    {
        var q = query.ToLowerInvariant().Trim();
        if (string.IsNullOrWhiteSpace(q))
            return (jobs, "Showing all open positions");

        var filtered = jobs.AsEnumerable();
        var parts = new List<string>();

        if (q.Contains("remote") || q.Contains("ghar se") || q.Contains("work from home"))
        {
            filtered = filtered.Where(j => j.IsRemote || j.Workplace == "Remote" || j.Workplace == "Hybrid");
            parts.Add("remote/hybrid roles");
        }

        if (q.Contains("fresher") || q.Contains("0-1") || q.Contains("entry"))
        {
            filtered = filtered.Where(j => j.Experience.StartsWith("0") || j.Experience.StartsWith("1") || j.Experience.StartsWith("2"));
            parts.Add("early career");
        }

        if (q.Contains("intern"))
        {
            filtered = filtered.Where(j => j.JobType.Contains("Intern", StringComparison.OrdinalIgnoreCase));
            parts.Add("internships");
        }

        foreach (var keyword in new[] { "react", "frontend", "backend", "devops", "java", "sales", "engineer", "developer", "analyst" })
        {
            if (q.Contains(keyword))
            {
                filtered = filtered.Where(j =>
                    j.Title.ToLowerInvariant().Contains(keyword) ||
                    j.Skills.Any(s => s.ToLowerInvariant().Contains(keyword)) ||
                    (j.Requirements ?? "").ToLowerInvariant().Contains(keyword));
                parts.Add(keyword);
            }
        }

        if (q.Contains("pune")) { filtered = filtered.Where(j => j.Location.Contains("Pune", StringComparison.OrdinalIgnoreCase)); parts.Add("Pune"); }
        if (q.Contains("mumbai")) { filtered = filtered.Where(j => j.Location.Contains("Mumbai", StringComparison.OrdinalIgnoreCase)); parts.Add("Mumbai"); }

        if (q.Contains("10 lpa") || q.Contains("10lpa") || q.Contains("salary"))
        {
            filtered = filtered.Where(j => j.SalaryRange.Contains("8") || j.SalaryRange.Contains("12") || j.SalaryRange.Contains("18"));
            parts.Add("competitive salary");
        }

        if (!parts.Any())
        {
            filtered = jobs.Where(j =>
                j.Title.ToLowerInvariant().Contains(q) ||
                j.Department.ToLowerInvariant().Contains(q) ||
                j.Skills.Any(s => s.ToLowerInvariant().Contains(q)));
            parts.Add($"search: \"{query}\"");
        }

        return (filtered.ToList(), string.Join(" · ", parts.Distinct()));
    }

    private static Dictionary<string, string> ParseFilterLabels(string message)
    {
        var q = message.ToLowerInvariant();
        var filters = new Dictionary<string, string>();
        if (q.Contains("remote")) filters["workplace"] = "Remote/Hybrid";
        if (q.Contains("react")) filters["skills"] = "React";
        if (q.Contains("pune")) filters["location"] = "Pune";
        if (q.Contains("fresher") || q.Contains("0-1")) filters["experience"] = "0-1 Years";
        return filters;
    }

    private static string CombineText(string? a, string? b) => $"{a} {b}".Trim();

    private static List<string> ExtractSkillsFromText(string text) =>
        text.Split(new[] { ',', '·', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 2 && s.Length < 20)
            .Take(8)
            .ToList();

    public async Task<LoginResponse> RegisterCandidateAsync(CareerRegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email already registered. Please sign in instead.");

        var candidateRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Candidate")
            ?? throw new InvalidOperationException("Candidate role is not configured.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            UserRoles = new List<UserRole> { new() { Role = candidateRole } }
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await LinkApplicationsToUserAsync(user.Id, user.Email);

        await _context.Entry(user).Collection(u => u.UserRoles).Query().Include(ur => ur.Role).LoadAsync();
        return GenerateLoginResponse(user);
    }

    public async Task<LoginResponse> LoginCandidateAsync(CareerLoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.UserRoles.Any(ur => ur.Role.Name == "Candidate"))
            throw new UnauthorizedAccessException("This account is not a candidate portal account.");

        user.LastLoginAt = DateTime.UtcNow;
        await LinkApplicationsToUserAsync(user.Id, user.Email);
        await _context.SaveChangesAsync();

        return GenerateLoginResponse(user);
    }

    public async Task<CareerApplyResponse> ApplyAsync(CareerApplyRequest request, int? userId)
    {
        var job = await GetJobAsync(request.JobRequisitionId)
            ?? throw new InvalidOperationException("This job is no longer open.");

        User? user = null;
        if (userId.HasValue)
        {
            user = await _context.Users.FindAsync(userId.Value);
            if (user != null)
            {
                request.Email = user.Email;
                request.FirstName = user.FirstName;
                request.LastName = user.LastName;
            }
        }

        var duplicateQuery = _context.CandidateProfiles
            .Where(c => c.JobRequisitionId == request.JobRequisitionId && c.Status != "Rejected");

        if (userId.HasValue)
            duplicateQuery = duplicateQuery.Where(c => c.UserId == userId || c.Email == request.Email);
        else
            duplicateQuery = duplicateQuery.Where(c => c.Email == request.Email);

        if (await duplicateQuery.AnyAsync())
            throw new InvalidOperationException("You have already applied for this role.");

        var match = await ComputeMatchAsync(new CareerMatchRequest
        {
            JobId = request.JobRequisitionId,
            ResumeText = request.ResumeText,
            SkillsText = request.SkillsText
        });

        var screeningSummary = match.Summary;
        var resumeText = request.ResumeText?.Trim() ?? "";
        if (resumeText.Length > 80)
        {
            try
            {
                var jd = $"{job.Title}\n{job.Description}\n{job.Requirements}";
                screeningSummary = await _aiService.ScreenCandidateAsync(jd, resumeText);
            }
            catch
            {
                screeningSummary = match.Summary;
            }
        }

        var candidate = new CandidateProfile
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            JobRequisitionId = request.JobRequisitionId,
            Status = "Screening",
            Source = "Career Portal",
            MatchScore = match.MatchPercent,
            ResumeText = resumeText,
            ScreeningSummary = screeningSummary,
            UserId = userId,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CandidateProfiles.Add(candidate);
        await _context.SaveChangesAsync();

        return new CareerApplyResponse
        {
            ApplicationId = candidate.Id,
            Status = candidate.Status,
            MatchScore = match.MatchPercent,
            ShortlistProbability = Math.Min(95, match.MatchPercent + 5),
            Message = "Application received! AI screening complete — HR review typically takes 2-3 business days."
        };
    }

    public async Task<List<CareerApplicationDto>> GetApplicationsAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return new List<CareerApplicationDto>();

        var apps = await _context.CandidateProfiles
            .Include(c => c.JobRequisition)
                .ThenInclude(j => j!.Department)
            .Where(c => c.UserId == userId || (c.UserId == null && c.Email == user.Email))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var linked = false;
        foreach (var app in apps.Where(a => a.UserId == null))
        {
            app.UserId = userId;
            linked = true;
        }
        if (linked)
            await _context.SaveChangesAsync();

        return apps.Select(MapApplication).ToList();
    }

    public async Task<CareerApplicationDto?> GetApplicationAsync(int userId, int applicationId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        var app = await _context.CandidateProfiles
            .Include(c => c.JobRequisition)
                .ThenInclude(j => j!.Department)
            .FirstOrDefaultAsync(c => c.Id == applicationId && (c.UserId == userId || c.Email == user.Email));

        return app == null ? null : MapApplication(app);
    }

    public async Task<CandidateProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        var profile = await _context.CandidateMasterProfiles
            .FirstOrDefaultAsync(mp => mp.UserId == userId);

        return profile == null ? new CandidateProfileDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
        } : MapProfileDto(profile, user);
    }

    public async Task<CandidateProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found");

        var profile = await _context.CandidateMasterProfiles
            .FirstOrDefaultAsync(mp => mp.UserId == userId);

        if (profile == null)
        {
            profile = new CandidateMasterProfile { UserId = userId };
            _context.CandidateMasterProfiles.Add(profile);
        }

        profile.Phone = request.Phone;
        profile.Gender = request.Gender;
        profile.DateOfBirth = request.DateOfBirth;
        profile.Nationality = request.Nationality;
        profile.CurrentAddress = request.CurrentAddress;
        profile.City = request.City;
        profile.State = request.State;
        profile.Country = request.Country;
        profile.ZipCode = request.ZipCode;
        profile.ProfessionalStatus = request.ProfessionalStatus;
        profile.CurrentCompany = request.CurrentCompany;
        profile.CurrentDesignation = request.CurrentDesignation;
        profile.TotalExperienceMonths = request.TotalExperienceMonths;
        profile.CurrentCtc = request.CurrentCtc;
        profile.ExpectedCtc = request.ExpectedCtc;
        profile.LinkedInUrl = request.LinkedInUrl;
        profile.GitHubUrl = request.GitHubUrl;
        profile.PortfolioUrl = request.PortfolioUrl;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapProfileDto(profile, user);
    }

    public async Task<CandidateProfileDto> UploadProfileResumeAsync(int userId, IFormFile file)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".pdf")
            throw new InvalidOperationException("Only PDF files are allowed");

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var text = ResumeTextExtractor.Extract(filePath);

        var profile = await _context.CandidateMasterProfiles
            .FirstOrDefaultAsync(mp => mp.UserId == userId);

        if (profile == null)
        {
            profile = new CandidateMasterProfile { UserId = userId };
            _context.CandidateMasterProfiles.Add(profile);
        }

        profile.ResumePath = $"/uploads/profiles/{fileName}";
        profile.ResumeFileName = file.FileName;
        profile.ResumeText = text;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapProfileDto(profile, user);
    }

    public async Task<UpdateProfileRequest?> ParseProfileResumeAsync(int userId)
    {
        var profile = await _context.CandidateMasterProfiles
            .FirstOrDefaultAsync(mp => mp.UserId == userId);

        if (profile == null || string.IsNullOrEmpty(profile.ResumeText))
            return null;

        return await _groqParser.ParseResumeAsync(profile.ResumeText);
    }

    public async Task<ParsedResumeData?> ParseProfileResumeFullAsync(int userId)
    {
        var profile = await _context.CandidateMasterProfiles
            .FirstOrDefaultAsync(mp => mp.UserId == userId);

        if (profile == null || string.IsNullOrEmpty(profile.ResumeText))
            return null;

        return await _groqParser.ParseResumeFullAsync(profile.ResumeText);
    }

    private static CandidateProfileDto MapProfileDto(CandidateMasterProfile profile, User user) => new()
    {
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Phone = profile.Phone,
        Gender = profile.Gender,
        DateOfBirth = profile.DateOfBirth,
        Nationality = profile.Nationality,
        CurrentAddress = profile.CurrentAddress,
        City = profile.City,
        State = profile.State,
        Country = profile.Country,
        ZipCode = profile.ZipCode,
        ProfessionalStatus = profile.ProfessionalStatus,
        CurrentCompany = profile.CurrentCompany,
        CurrentDesignation = profile.CurrentDesignation,
        TotalExperienceMonths = profile.TotalExperienceMonths,
        CurrentCtc = profile.CurrentCtc,
        ExpectedCtc = profile.ExpectedCtc,
        LinkedInUrl = profile.LinkedInUrl,
        GitHubUrl = profile.GitHubUrl,
        PortfolioUrl = profile.PortfolioUrl,
        ResumePath = profile.ResumePath,
        ResumeFileName = profile.ResumeFileName,
    };

    private async Task LinkApplicationsToUserAsync(int userId, string email)
    {
        var orphanApps = await _context.CandidateProfiles
            .Where(c => c.Email == email && c.UserId == null)
            .ToListAsync();

        foreach (var app in orphanApps)
            app.UserId = userId;

        if (orphanApps.Count > 0)
            await _context.SaveChangesAsync();
    }

    private LoginResponse GenerateLoginResponse(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "SuperSecretKeyForHRMS2024Minimum32Chars!");
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };
        claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _configuration["Jwt:Issuer"] ?? "HRMS.API",
            Audience = _configuration["Jwt:Audience"] ?? "HRMS.App",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        return new LoginResponse
        {
            Token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor)),
            RefreshToken = "",
            ExpiresAt = expiresAt,
            User = user.ToDto()
        };
    }

    private static CareerApplicationDto MapApplication(CandidateProfile app)
    {
        var job = app.JobRequisition;
        var pipeline = BuildPipeline(app.Status);
        var current = pipeline.FirstOrDefault(s => s.Current);

        return new CareerApplicationDto
        {
            Id = app.Id,
            JobId = app.JobRequisitionId ?? 0,
            JobTitle = job?.Title ?? "Role",
            Department = job?.Department?.Name ?? "General",
            Location = job != null ? EnrichJob(job).Location : "Pune",
            Status = app.Status,
            StatusLabel = GetStatusLabel(app.Status),
            MatchScore = app.MatchScore.HasValue ? (int?)Math.Round(app.MatchScore.Value) : null,
            AppliedAt = app.CreatedAt,
            UpdatedAt = app.UpdatedAt,
            NextStepHint = GetNextStepHint(app.Status),
            Pipeline = pipeline
        };
    }

    private static string GetStatusLabel(string status) => status switch
    {
        "New" => "Application Received",
        "Screening" => "AI Screening Complete",
        "Interviewed" => "In Interviews",
        "Offered" => "Offer Extended",
        "Hired" => "Hired",
        "Rejected" => "Not Selected",
        _ => status
    };

    private static string GetNextStepHint(string status) => status switch
    {
        "New" => "AI screening will begin shortly.",
        "Screening" => "Our HR team is reviewing your profile — expect an update in 2-3 business days.",
        "Interviewed" => "Interview rounds in progress. Check your email for schedule details.",
        "Offered" => "Review your offer letter and respond at your earliest convenience.",
        "Hired" => "Welcome! Onboarding tasks will be shared soon.",
        "Rejected" => "Thank you for your interest. You may apply for other open roles.",
        _ => "We will update you on next steps."
    };

    private static List<CareerApplicationStageDto> BuildPipeline(string status)
    {
        var stages = new[]
        {
            ("Applied", "Application submitted"),
            ("AI Screening", "Resume matched against job description"),
            ("HR Review", "Recruiter validates your profile"),
            ("Interview", "Technical and manager rounds"),
            ("Offer", "Offer letter and negotiation"),
            ("Hired", "Welcome to the team")
        };

        var currentIndex = status switch
        {
            "New" => 0,
            "Screening" => 2,
            "Interviewed" => 3,
            "Offered" => 4,
            "Hired" => 5,
            "Rejected" => -1,
            _ => 1
        };

        return stages.Select((stage, index) => new CareerApplicationStageDto
        {
            Title = stage.Item1,
            Description = stage.Item2,
            Completed = currentIndex >= 0 && index < currentIndex,
            Current = currentIndex >= 0 && index == currentIndex
        }).ToList();
    }
}
