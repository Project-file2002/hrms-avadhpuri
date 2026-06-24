using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Career;
using HRMS.API.Services.Interfaces;
using HRMS.API.Utils;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CareersController : ControllerBase
{
    private readonly ICareerPortalService _careerPortal;

    public CareersController(ICareerPortalService careerPortal)
    {
        _careerPortal = careerPortal;
    }

    [AllowAnonymous]
    [HttpPost("extract-text")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public IActionResult ExtractText(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".pdf" && ext != ".doc" && ext != ".docx")
            return BadRequest(new { message = "Only PDF, DOC, DOCX allowed" });

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            file.CopyTo(stream);

        var text = ResumeTextExtractor.Extract(filePath);

        try { System.IO.File.Delete(filePath); } catch { /* cleanup */ }

        return Ok(new { text });
    }

    [AllowAnonymous]
    [HttpGet("config")]
    public async Task<IActionResult> GetConfig() => Ok(await _careerPortal.GetConfigAsync());

    [AllowAnonymous]
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs() => Ok(await _careerPortal.GetJobsAsync());

    [AllowAnonymous]
    [HttpGet("jobs/{id:int}")]
    public async Task<IActionResult> GetJob(int id)
    {
        var job = await _careerPortal.GetJobAsync(id);
        return job == null ? NotFound() : Ok(job);
    }

    [AllowAnonymous]
    [HttpPost("match")]
    public async Task<IActionResult> Match([FromBody] CareerMatchRequest request) =>
        Ok(await _careerPortal.ComputeMatchAsync(request));

    [AllowAnonymous]
    [HttpPost("explain")]
    public async Task<IActionResult> Explain([FromBody] CareerExplainJobRequest request) =>
        Ok(await _careerPortal.ExplainJobAsync(request));

    [AllowAnonymous]
    [HttpPost("resume-review")]
    public async Task<IActionResult> ResumeReview([FromBody] CareerResumeReviewRequest request) =>
        Ok(await _careerPortal.ReviewResumeAsync(request));

    [AllowAnonymous]
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] CareerSearchRequest request) =>
        Ok(await _careerPortal.SearchJobsAsync(request));

    [AllowAnonymous]
    [HttpPost("assistant")]
    public async Task<IActionResult> Assistant([FromBody] CareerAssistantRequest request) =>
        Ok(await _careerPortal.AssistantQueryAsync(request));

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CareerRegisterRequest request)
    {
        try
        {
            return Ok(await _careerPortal.RegisterCandidateAsync(request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] CareerLoginRequest request)
    {
        try
        {
            return Ok(await _careerPortal.LoginCandidateAsync(request));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize(Roles = "Candidate")]
    public IActionResult Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(new
        {
            id = userId,
            email = User.FindFirstValue(ClaimTypes.Email),
            name = User.FindFirstValue(ClaimTypes.Name)
        });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _careerPortal.GetProfileAsync(userId);
        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var profile = await _careerPortal.UpdateProfileAsync(userId, request);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("profile/test-groq")]
    public async Task<IActionResult> TestGroq()
    {
        var apiKey = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Groq:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            return BadRequest(new { message = "GROQ_API_KEY not found in .env or appsettings" });

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var db = HttpContext.RequestServices.GetRequiredService<HRMSDbContext>();
        var profile = await db.CandidateMasterProfiles.FirstOrDefaultAsync(mp => mp.UserId == userId);

        // Direct Groq API test to get actual response
        try
        {
            var http = HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();
            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[] { new { role = "user", content = "Say 'Groq API is working' and nothing else." } },
                temperature = 0.1,
                max_tokens = 50,
            };
            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            req.Headers.Add("Authorization", $"Bearer {apiKey}");
            req.Content = new StringContent(json, Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            var res = await http.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();
            return Ok(new
            {
                message = res.IsSuccessStatusCode ? "Groq API responded successfully" : $"Groq returned status {res.StatusCode}",
                statusCode = (int)res.StatusCode,
                success = res.IsSuccessStatusCode,
                responseBody = body,
                apiKeyPrefix = apiKey[..Math.Min(8, apiKey.Length)] + "...",
                hasResumeText = !string.IsNullOrEmpty(profile?.ResumeText),
                resumeTextLength = profile?.ResumeText?.Length ?? 0,
            });
        }
        catch (Exception ex)
        {
            return Ok(new { message = $"Exception: {ex.Message}", detail = ex.ToString() });
        }
    }

    [HttpPost("profile/parse-resume")]
    public async Task<IActionResult> ParseProfileResume()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var parsed = await _careerPortal.ParseProfileResumeAsync(userId);
            if (parsed == null)
                return BadRequest(new { message = "Parsing returned null. Check your Groq API key is valid and has credits." });
            return Ok(parsed);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("profile/parse-resume-full")]
    public async Task<IActionResult> ParseProfileResumeFull()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var parsed = await _careerPortal.ParseProfileResumeFullAsync(userId);
            if (parsed == null)
                return BadRequest(new { message = "Full parsing returned null. Check your Groq API key is valid and has credits." });
            return Ok(parsed);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("profile/upload-resume")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfileResume(IFormFile file)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var profile = await _careerPortal.UploadProfileResumeAsync(userId, file);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("applications")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetApplications()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _careerPortal.GetApplicationsAsync(userId));
    }

    [HttpGet("applications/{id:int}")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetApplication(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var app = await _careerPortal.GetApplicationAsync(userId, id);
        return app == null ? NotFound() : Ok(app);
    }

    [Authorize(Roles = "Candidate")]
    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] CareerApplyRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            return Ok(await _careerPortal.ApplyAsync(request, userId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
