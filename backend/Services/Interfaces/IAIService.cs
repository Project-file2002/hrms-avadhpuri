namespace HRMS.API.Services.Interfaces;

public interface IAIService
{
    Task<string> ScreenCandidateAsync(string jobDescription, string resumeText);
    Task<string> GenerateInterviewQuestionsAsync(string role, string candidateProfile);
    Task<string> SummarizeFeedbackAsync(string feedbackNotes);
    Task<string> GetRecommendationAsync(string candidateData, string jobRequirements);
    Task<string> GetResponseAsync(string message, Models.Entities.Employee employee, Models.Entities.User user, IEnumerable<string> roles);
    Task<string> GenerateMeetingAgendaAsync(string title, string meetingType, string? description);
    Task<string> GenerateMomTemplateAsync(string title, string meetingType);
    Task<string> SummarizeMeetingAsync(string title, string notes);
    Task<int> SuggestMeetingDurationAsync(string meetingType, int participantCount);
}
