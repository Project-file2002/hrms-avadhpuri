using HRMS.API.Models.DTOs.Recruitment;

namespace HRMS.API.Services.Interfaces;

public interface IRecruitmentService
{
    Task<IEnumerable<JobRequisitionDto>> GetOpenJobsAsync();
    Task<IEnumerable<CandidateDto>> GetCandidatesAsync();
    Task<CandidateDto?> GetCandidateByIdAsync(int id);
    Task<CandidateDto> CreateCandidateAsync(CreateCandidate request);
    Task<IEnumerable<JobRequisitionDto>> GetJobRequisitionsAsync();
    Task<JobRequisitionDto> CreateJobRequisitionAsync(CreateJobRequisition request);
    Task<InterviewDto> ScheduleInterviewAsync(CreateInterview request);
    Task<IEnumerable<InterviewDto>> GetInterviewsAsync(int candidateId);
}
