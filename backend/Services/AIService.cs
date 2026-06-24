using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HRMS.API.Data;
using HRMS.API.Models.Entities;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Services;

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly HRMSDbContext _context;
    private readonly string? _apiKey;
    private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";

    public AIService(HttpClient httpClient, IConfiguration configuration, HRMSDbContext context)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _context = context;
        _apiKey = configuration["AI:ApiKey"];
    }

    public async Task<string> ScreenCandidateAsync(string jobDescription, string resumeText)
    {
        var prompt = $@"You are an HR recruitment assistant. Analyze the following candidate's resume against the job description.
Job Description: {jobDescription}
Resume: {resumeText}

Provide:
1. Match score (0-100)
2. Key strengths
3. Potential gaps
4. Overall recommendation";

        return await CallAIAsync("candidate_screening", prompt);
    }

    public async Task<string> GenerateInterviewQuestionsAsync(string role, string candidateProfile)
    {
        var prompt = $@"You are an interview coach. Generate relevant interview questions for the following:
Role: {role}
Candidate Profile: {candidateProfile}

Generate 5-7 technical and behavioral questions tailored to this role and candidate background.";

        return await CallAIAsync("interview_questions", prompt);
    }

    public async Task<string> SummarizeFeedbackAsync(string feedbackNotes)
    {
        var prompt = $@"Summarize the following interview feedback into a concise evaluation:
{feedbackNotes}

Provide:
1. Overall assessment
2. Key strengths observed
3. Areas for improvement
4. Hiring recommendation (Strong Yes / Yes / Maybe / No)";

        return await CallAIAsync("feedback_summary", prompt);
    }

    public async Task<string> GetResponseAsync(string message, Employee employee, User user, IEnumerable<string> roles)
    {
        var roleList = roles.ToList();
        var persona = CopilotPermissions.GetPrimaryPersona(roleList);
        var restrictions = persona switch
        {
            "employee" =>
                "STRICT RULES: You are an Employee HR Assistant. ONLY discuss the user's personal HR data (their leave, attendance, performance, salary, profile, policies). " +
                "NEVER reveal other employees' data, team stats, headcount, recruitment, or company-wide analytics. Redirect those requests to their manager or HR.",
            "manager" =>
                "STRICT RULES: You are a Manager HR Copilot. You may discuss the user's personal data AND their direct team's attendance/members/performance. " +
                "Do NOT share company-wide recruitment costs, payroll of other employees, or data outside their team unless they are also HR/Admin.",
            "hr" =>
                "STRICT RULES: You are an HR Copilot with org-wide workforce access (employees, leave trends, recruitment, departments, policies). " +
                "Do not share system admin settings or unrelated employee salary details unless asked in an HR context.",
            "payroll" =>
                "STRICT RULES: You are a Payroll Copilot. Focus on payroll summaries, headcount, policies, and the user's profile. " +
                "Do NOT discuss recruitment pipeline, team management, or performance reviews of others.",
            _ =>
                "STRICT RULES: You are an Admin HR Copilot with full workforce access. Provide accurate, concise HR guidance."
        };

        var prompt = $@"You are an AI HR Copilot for an Enterprise Workforce Experience Platform.
{restrictions}

The user is: {employee.FirstName} {employee.LastName}, {employee.Position}, Department: {employee.Department?.Name ?? "N/A"}
Their roles: {string.Join(", ", roleList)}
Persona mode: {persona}

The user may speak Hinglish (Hindi + English mix). Respond helpfully and concisely in the same language style.

User message: {message}

If the request is outside their role's data access, politely explain what you CAN help with instead.";
        return await CallAIAsync("copilot_general", prompt);
    }

    public async Task<string> GetRecommendationAsync(string candidateData, string jobRequirements)
    {
        var prompt = $@"Based on the following candidate data and job requirements, provide a hiring recommendation:
Candidate: {candidateData}
Job Requirements: {jobRequirements}

Provide a clear recommendation with reasoning.";

        return await CallAIAsync("recommendation", prompt);
    }

    public async Task<string> GenerateMeetingAgendaAsync(string title, string meetingType, string? description)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return BuildFallbackAgenda(title, meetingType, description);

        var prompt = $@"Generate a structured meeting agenda for:
Title: {title}
Type: {meetingType}
Context: {description ?? "N/A"}

Include: Objectives, Talking Points, Risks, Action Items (as bullet lists). Keep it concise and actionable.";
        var result = await CallAIAsync("meeting_agenda", prompt);
        if (result.StartsWith("AI API key") || result.StartsWith("AI service error") || result.StartsWith("Error"))
            return BuildFallbackAgenda(title, meetingType, description);
        return result;
    }

    public async Task<string> GenerateMomTemplateAsync(string title, string meetingType)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return BuildFallbackMom(title, meetingType);

        var prompt = $@"Create a Minutes of Meeting (MoM) template for:
Title: {title}
Type: {meetingType}

Include sections: Attendees, Agenda Covered, Key Decisions, Action Items (Owner + Deadline), Next Steps.";
        var result = await CallAIAsync("meeting_mom", prompt);
        if (result.StartsWith("AI API key") || result.StartsWith("AI service error") || result.StartsWith("Error"))
            return BuildFallbackMom(title, meetingType);
        return result;
    }

    public async Task<string> SummarizeMeetingAsync(string title, string notes)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return $"Meeting Summary — {title}\n\n{notes}\n\nKey outcomes and follow-ups should be tracked as tasks.";

        var prompt = $@"Summarize this meeting into concise MoM format:
Meeting: {title}
Notes: {notes}

Provide: Summary, Decisions, Action Items.";
        return await CallAIAsync("meeting_summary", prompt);
    }

    public Task<int> SuggestMeetingDurationAsync(string meetingType, int participantCount)
    {
        var baseMinutes = meetingType switch
        {
            "1:1" or "Performance Review" => 30,
            "Interview" => 45,
            "Town Hall" or "Training" => 60,
            "Sprint" or "Weekly Review" => 60,
            "Board Meeting" or "Executive Meeting" => 90,
            _ => 45
        };
        if (participantCount > 8) baseMinutes += 15;
        return Task.FromResult(baseMinutes);
    }

    private static string BuildFallbackAgenda(string title, string meetingType, string? description) =>
        $@"# Agenda: {title}
Type: {meetingType}

## Objectives
- Align on priorities for {title}
- Review progress and blockers
{(description != null ? $"- Address: {description}" : "")}

## Talking Points
- Previous action items status
- Current sprint/project updates
- Risks and dependencies
- Q&A / open discussion

## Risks
- Schedule conflicts or resource gaps
- Unresolved blockers from last meeting

## Action Items
- [ ] Document decisions and assign owners
- [ ] Schedule follow-up if needed";

    private static string BuildFallbackMom(string title, string meetingType) =>
        $@"# Minutes of Meeting — {title}
Type: {meetingType}
Date: {DateTime.UtcNow:yyyy-MM-dd}

## Attendees
- (List participants)

## Agenda Covered
1.
2.

## Key Decisions
-

## Action Items
| Task | Owner | Deadline |
|------|-------|----------|
|      |       |          |

## Next Steps
-";

    private async Task<string> CallAIAsync(string feature, string prompt)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return "AI API key not configured. Please set AI:ApiKey in configuration.";

        try
        {
            var requestBody = new
            {
                model = "llama3-70b-8192",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful HR assistant." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 500,
                temperature = 0.7
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = httpContent;

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await LogAIUsage(feature, prompt, $"Error: {response.StatusCode} - {responseBody}", false);
                return $"AI service error: {response.StatusCode}";
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var text = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

            await LogAIUsage(feature, prompt, text, true);
            return text;
        }
        catch (Exception ex)
        {
            await LogAIUsage(feature, prompt, $"Exception: {ex.Message}", false);
            return $"Error calling AI service: {ex.Message}";
        }
    }

    private async Task LogAIUsage(string feature, string prompt, string response, bool success)
    {
        try
        {
            _context.AIUsageLogs.Add(new AIUsageLog
            {
                Feature = feature,
                Prompt = prompt,
                Response = response,
                Success = success,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
        catch
        {
        }
    }
}
