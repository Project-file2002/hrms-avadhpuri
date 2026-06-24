using System.Text;
using System.Text.Json;
using HRMS.API.Models.DTOs.Career;

namespace HRMS.API.Services;

public interface IGroqResumeParser
{
    Task<UpdateProfileRequest?> ParseResumeAsync(string resumeText);
    Task<ParsedResumeData?> ParseResumeFullAsync(string resumeText);
}

public class GroqResumeParser : IGroqResumeParser
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public GroqResumeParser(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    private async Task<string?> CallGroqAsync(string prompt, int maxTokens = 4096)
    {
        var apiKey = _config["Groq:ApiKey"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            return null;

        var requestBody = new
        {
            model = "llama-3.3-70b-versatile",
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.1,
            max_tokens = maxTokens,
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOpts);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        request.Content = content;

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        var messageContent = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return messageContent;
    }

    private static string? CleanJson(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;
        var cleaned = raw.Trim();
        if (cleaned.StartsWith("```json")) cleaned = cleaned[7..];
        else if (cleaned.StartsWith("```")) cleaned = cleaned[3..];
        if (cleaned.EndsWith("```")) cleaned = cleaned[..^3];
        return cleaned.Trim();
    }

    public async Task<UpdateProfileRequest?> ParseResumeAsync(string resumeText)
    {
        var prompt = $@"You are a resume parser. Extract the following fields from the resume text below and return ONLY a valid JSON object (no markdown, no code fences). Use null for missing fields.

Fields to extract:
- phone (string)
- gender (string: Male/Female/Other/PreferNotToSay)
- dateOfBirth (ISO date string or null)
- nationality (string)
- currentAddress (string)
- city (string)
- state (string)
- country (string)
- zipCode (string)
- professionalStatus (string: Student/Fresher/Working/ServingNotice/Freelancer/CareerBreak/LookingInternship)
- currentCompany (string)
- currentDesignation (string)
- totalExperienceMonths (number, total experience in months)
- currentCtc (number, current salary in LPA)
- expectedCtc (number, expected salary in LPA)
- linkedInUrl (string)
- gitHubUrl (string)
- portfolioUrl (string)

Resume text:
---
{resumeText}
---";

        var raw = await CallGroqAsync(prompt, 1024);
        var cleaned = CleanJson(raw);
        if (cleaned == null) return null;

        try { return JsonSerializer.Deserialize<UpdateProfileRequest>(cleaned, JsonOpts); }
        catch { return null; }
    }

    public async Task<ParsedResumeData?> ParseResumeFullAsync(string resumeText)
    {
        var prompt = $@"You are a resume parser. Extract ALL the following fields from the resume text below and return ONLY a valid JSON object (no markdown, no code fences). Use null for missing fields. For array fields, include ALL entries found in the resume (empty array if none).

Single fields:
- firstName (string)
- lastName (string)
- email (string)
- phone (string)
- gender (string: Male/Female/Other/PreferNotToSay)
- dateOfBirth (ISO date string or null, e.g. ""1995-06-15"")
- nationality (string)
- currentAddress (string)
- city (string)
- state (string)
- country (string)
- zipCode (string)
- professionalStatus (string: Student/Fresher/Working/ServingNotice/Freelancer/CareerBreak/LookingInternship)
- currentCompany (string)
- currentDesignation (string)
- totalExperienceMonths (number, total professional experience in months)
- currentCtc (number, current annual salary in LPA)
- expectedCtc (number, expected annual salary in LPA)
- linkedInUrl (string)
- gitHubUrl (string)
- portfolioUrl (string)

Education array (education):
  - degree (string, e.g. B.Tech, MBA)
  - institution (string)
  - yearOfPassing (number)
  - percentage (string, e.g. ""85%"", ""8.5 CGPA"")
  - specialization (string)
  - isPursuing (boolean)

Experience array (experience):
  - company (string)
  - role (string)
  - startDate (string, e.g. ""2019-06"")
  - endDate (string, e.g. ""2023-08"" or null if current)
  - isCurrent (boolean)
  - responsibilities (string)

Projects array (projects):
  - title (string)
  - description (string)
  - techStack (string)
  - url (string)
  - duration (string)

Internships array (internships):
  - company (string)
  - role (string)
  - duration (string)
  - description (string)

Skills array (skills):
  - skill (string)
  - proficiency (string: Beginner/Intermediate/Advanced/Expert)
  - yearsOfExperience (number)

Certifications array (certifications):
  - name (string)
  - issuer (string)
  - year (number)
  - url (string)

Training array (training):
  - title (string)
  - provider (string)
  - year (number)
  - duration (string)

Achievements array (achievements):
  - type (string: Award/Publication/Patent/Hackathon/OpenSource/Other)
  - title (string)
  - organization (string)
  - year (number)
  - description (string)
  - url (string)

Languages array (languages):
  - language (string)
  - proficiency (string: Basic/Conversational/Professional/Native)
  - canRead (boolean)
  - canWrite (boolean)
  - canSpeak (boolean)

References array (references):
  - name (string)
  - company (string)
  - role (string)
  - email (string)
  - phone (string)

Resume text:
---
{resumeText}
---";

        var raw = await CallGroqAsync(prompt, 4096);
        var cleaned = CleanJson(raw);
        if (cleaned == null) return null;

        try { return JsonSerializer.Deserialize<ParsedResumeData>(cleaned, JsonOpts); }
        catch { return null; }
    }
}
