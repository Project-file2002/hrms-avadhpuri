using HRMS.API.Models.DTOs.Auth;
using HRMS.API.Models.DTOs.Career;

namespace HRMS.API.Services.Interfaces;

public interface ICareerPortalService
{
    Task<CareerPortalConfigDto> GetConfigAsync();
    Task<List<CareerJobListingDto>> GetJobsAsync();
    Task<CareerJobListingDto?> GetJobAsync(int id);
    Task<CareerMatchResponse> ComputeMatchAsync(CareerMatchRequest request);
    Task<CareerExplainJobResponse> ExplainJobAsync(CareerExplainJobRequest request);
    Task<CareerResumeReviewResponse> ReviewResumeAsync(CareerResumeReviewRequest request);
    Task<CareerSearchResponse> SearchJobsAsync(CareerSearchRequest request);
    Task<CareerAssistantResponse> AssistantQueryAsync(CareerAssistantRequest request);
    Task<LoginResponse> RegisterCandidateAsync(CareerRegisterRequest request);
    Task<LoginResponse> LoginCandidateAsync(CareerLoginRequest request);
    Task<CareerApplyResponse> ApplyAsync(CareerApplyRequest request, int? userId);
    Task<List<CareerApplicationDto>> GetApplicationsAsync(int userId);
    Task<CareerApplicationDto?> GetApplicationAsync(int userId, int applicationId);
    Task<CandidateProfileDto?> GetProfileAsync(int userId);
    Task<CandidateProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<CandidateProfileDto> UploadProfileResumeAsync(int userId, IFormFile file);
    Task<UpdateProfileRequest?> ParseProfileResumeAsync(int userId);
    Task<ParsedResumeData?> ParseProfileResumeFullAsync(int userId);
}