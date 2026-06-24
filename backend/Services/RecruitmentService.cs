using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Recruitment;
using HRMS.API.Models.Entities;
using HRMS.API.Models.Mapping;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Services;

public class RecruitmentService : IRecruitmentService
{
    private readonly HRMSDbContext _context;

    public RecruitmentService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JobRequisitionDto>> GetOpenJobsAsync()
    {
        var jobs = await _context.JobRequisitions
            .Where(jr => jr.Status == "Open")
            .OrderByDescending(jr => jr.CreatedAt)
            .ToListAsync();
        return jobs.Select(jr => jr.ToDto());
    }

    public async Task<IEnumerable<CandidateDto>> GetCandidatesAsync()
    {
        var candidates = await _context.CandidateProfiles
            .Include(cp => cp.JobRequisition)
            .OrderByDescending(cp => cp.CreatedAt)
            .ToListAsync();

        return candidates.Select(cp => cp.ToDto());
    }

    public async Task<CandidateDto?> GetCandidateByIdAsync(int id)
    {
        var candidate = await _context.CandidateProfiles
            .Include(cp => cp.JobRequisition)
            .FirstOrDefaultAsync(cp => cp.Id == id);

        return candidate?.ToDto();
    }

    public async Task<CandidateDto> CreateCandidateAsync(CreateCandidate request)
    {
        var candidate = new CandidateProfile
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Source = request.Source,
            JobRequisitionId = request.JobRequisitionId
        };

        _context.CandidateProfiles.Add(candidate);
        await _context.SaveChangesAsync();

        return candidate.ToDto();
    }

    public async Task<IEnumerable<JobRequisitionDto>> GetJobRequisitionsAsync()
    {
        var requisitions = await _context.JobRequisitions
            .Include(jr => jr.Candidates)
            .OrderByDescending(jr => jr.CreatedAt)
            .ToListAsync();

        return requisitions.Select(jr => jr.ToDto());
    }

    public async Task<JobRequisitionDto> CreateJobRequisitionAsync(CreateJobRequisition request)
    {
        var requisition = new JobRequisition
        {
            Title = request.Title,
            Description = request.Description,
            Requirements = request.Requirements,
            DepartmentId = request.DepartmentId
        };

        _context.JobRequisitions.Add(requisition);
        await _context.SaveChangesAsync();

        return requisition.ToDto();
    }

    public async Task<InterviewDto> ScheduleInterviewAsync(CreateInterview request)
    {
        var interview = new InterviewSchedule
        {
            CandidateId = request.CandidateId,
            ScheduledDate = request.ScheduledDate,
            InterviewerName = request.InterviewerName,
            InterviewType = request.InterviewType
        };

        _context.InterviewSchedules.Add(interview);
        await _context.SaveChangesAsync();

        return interview.ToDto();
    }

    public async Task<IEnumerable<InterviewDto>> GetInterviewsAsync(int candidateId)
    {
        var interviews = await _context.InterviewSchedules
            .Where(i => i.CandidateId == candidateId)
            .OrderByDescending(i => i.ScheduledDate)
            .ToListAsync();

        return interviews.Select(i => i.ToDto());
    }
}
