using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("screen")]
    public async Task<IActionResult> ScreenCandidate([FromBody] ScreenRequest request)
    {
        var result = await _aiService.ScreenCandidateAsync(request.JobDescription, request.ResumeText);
        return Ok(new { result });
    }

    [HttpPost("interview-questions")]
    public async Task<IActionResult> GenerateQuestions([FromBody] QuestionsRequest request)
    {
        var result = await _aiService.GenerateInterviewQuestionsAsync(request.Role, request.CandidateProfile);
        return Ok(new { result });
    }

    [HttpPost("summarize-feedback")]
    public async Task<IActionResult> SummarizeFeedback([FromBody] FeedbackRequest request)
    {
        var result = await _aiService.SummarizeFeedbackAsync(request.FeedbackNotes);
        return Ok(new { result });
    }

    [HttpPost("recommend")]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest request)
    {
        var result = await _aiService.GetRecommendationAsync(request.CandidateData, request.JobRequirements);
        return Ok(new { result });
    }
}

public class ScreenRequest
{
    public string JobDescription { get; set; } = string.Empty;
    public string ResumeText { get; set; } = string.Empty;
}

public class QuestionsRequest
{
    public string Role { get; set; } = string.Empty;
    public string CandidateProfile { get; set; } = string.Empty;
}

public class FeedbackRequest
{
    public string FeedbackNotes { get; set; } = string.Empty;
}

public class RecommendRequest
{
    public string CandidateData { get; set; } = string.Empty;
    public string JobRequirements { get; set; } = string.Empty;
}
