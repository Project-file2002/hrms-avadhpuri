using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;
using HRMS.API.Models.DTOs.Recruitment;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager,Manager")]
public class RecruitmentController : ControllerBase
{
    private readonly HRMSDbContext _context;

    public RecruitmentController(HRMSDbContext context) => _context = context;

    [AllowAnonymous]
    [HttpGet("jobs")]
    public async Task<IActionResult> GetOpenJobs()
    {
        var jobs = await _context.JobRequisitions.Where(j => j.Status == "Open")
            .Select(j => new { j.Id, j.Title, j.Description, j.Requirements, j.DepartmentId }).ToListAsync();
        return Ok(jobs);
    }

    [AllowAnonymous]
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyForJob([FromBody] CreateCandidate request)
    {
        var candidate = new CandidateProfile
        {
            FirstName = request.FirstName, LastName = request.LastName,
            Email = request.Email, Phone = request.Phone,
            JobRequisitionId = request.JobRequisitionId, Status = "New"
        };
        _context.CandidateProfiles.Add(candidate);
        await _context.SaveChangesAsync();
        return Ok(new { candidate.Id, candidate.Status });
    }

    [Authorize]
    [HttpGet("candidates")]
    public async Task<IActionResult> GetCandidates()
    {
        var candidates = await _context.CandidateProfiles
            .Include(c => c.JobRequisition)
            .Include(c => c.Offers)
            .Include(c => c.BackgroundChecks)
            .Include(c => c.OnboardingTasks)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return Ok(candidates.Select(MapCandidate));
    }

    [HttpGet("candidates/{id}")]
    public async Task<IActionResult> GetCandidateById(int id)
    {
        var c = await _context.CandidateProfiles
            .Include(x => x.JobRequisition)
            .Include(x => x.Offers)
            .Include(x => x.BackgroundChecks)
            .Include(x => x.OnboardingTasks)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        return Ok(MapCandidate(c));
    }

    [HttpPost("candidates")]
    public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidate request)
    {
        var candidate = new CandidateProfile
        {
            FirstName = request.FirstName, LastName = request.LastName,
            Email = request.Email, Phone = request.Phone,
            JobRequisitionId = request.JobRequisitionId, Status = "New", Source = request.Source
        };
        _context.CandidateProfiles.Add(candidate);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCandidateById), new { id = candidate.Id }, candidate);
    }

    [HttpPut("candidates/{id}/status")]
    public async Task<IActionResult> UpdateCandidateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var candidate = await _context.CandidateProfiles.FindAsync(id);
        if (candidate == null) return NotFound();
        candidate.Status = request.Status;
        candidate.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { id, candidate.Status });
    }

    [HttpGet("requisitions")]
    public async Task<IActionResult> GetRequisitions()
    {
        var requisitions = await _context.JobRequisitions
            .Include(j => j.Department)
            .Select(j => new {
                j.Id, j.Title, j.Description, j.Requirements, j.Status,
                j.CreatedAt, j.ClosedAt, j.DepartmentId,
                Department = j.Department != null ? j.Department.Name : null,
                CandidateCount = j.Candidates.Count
            }).OrderByDescending(j => j.CreatedAt).ToListAsync();
        return Ok(requisitions);
    }

    [HttpGet("requisitions/{id}")]
    public async Task<IActionResult> GetRequisitionById(int id)
    {
        var j = await _context.JobRequisitions
            .Include(j => j.Department)
            .Include(j => j.Candidates)
            .FirstOrDefaultAsync(j => j.Id == id);
        if (j == null) return NotFound();
        return Ok(new
        {
            j.Id, j.Title, j.Description, j.Requirements, j.Status,
            j.CreatedAt, j.ClosedAt, j.DepartmentId,
            Department = j.Department?.Name,
            CandidateCount = j.Candidates.Count
        });
    }

    [HttpPost("requisitions")]
    public async Task<IActionResult> CreateRequisition([FromBody] CreateJobRequisition request)
    {
        var requisition = new JobRequisition
        {
            Title = request.Title, Description = request.Description,
            Requirements = request.Requirements, DepartmentId = request.DepartmentId
        };
        _context.JobRequisitions.Add(requisition);
        await _context.SaveChangesAsync();
        return Ok(requisition);
    }

    [HttpPut("requisitions/{id}")]
    public async Task<IActionResult> UpdateRequisition(int id, [FromBody] UpdateJobRequisition request)
    {
        var requisition = await _context.JobRequisitions.FindAsync(id);
        if (requisition == null) return NotFound();
        requisition.Title = request.Title;
        requisition.Description = request.Description;
        requisition.Requirements = request.Requirements;
        requisition.DepartmentId = request.DepartmentId;
        if (request.Status == "Closed" && requisition.Status == "Open")
            requisition.ClosedAt = DateTime.UtcNow;
        else if (request.Status == "Open" && requisition.Status == "Closed")
            requisition.ClosedAt = null;
        requisition.Status = request.Status;
        await _context.SaveChangesAsync();
        return Ok(requisition);
    }

    [HttpDelete("requisitions/{id}")]
    public async Task<IActionResult> DeleteRequisition(int id)
    {
        var requisition = await _context.JobRequisitions.FindAsync(id);
        if (requisition == null) return NotFound();
        _context.JobRequisitions.Remove(requisition);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("interviews")]
    public async Task<IActionResult> ScheduleInterview([FromBody] CreateInterview request)
    {
        var interview = new InterviewSchedule
        {
            CandidateId = request.CandidateId, ScheduledDate = request.ScheduledDate,
            InterviewerName = request.InterviewerName, InterviewType = request.InterviewType
        };
        _context.InterviewSchedules.Add(interview);
        await _context.SaveChangesAsync();
        return Ok(interview);
    }

    [HttpGet("interviews/{candidateId}")]
    public async Task<IActionResult> GetInterviews(int candidateId)
    {
        var interviews = await _context.InterviewSchedules
            .Where(i => i.CandidateId == candidateId)
            .OrderBy(i => i.ScheduledDate).ToListAsync();
        return Ok(interviews);
    }

    // === Hiring Requests ===

    [HttpGet("hiring-requests")]
    public async Task<IActionResult> GetHiringRequests()
    {
        var requests = await _context.HiringRequests
            .Include(h => h.RequestedBy)
            .Include(h => h.Department)
            .Include(h => h.JobRequisition)
            .OrderByDescending(h => h.CreatedAt).ToListAsync();
        return Ok(requests.Select(MapHiringRequest));
    }

    [HttpPost("hiring-requests")]
    public async Task<IActionResult> CreateHiringRequest([FromBody] CreateHiringRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized("No linked employee");

        var hr = new HiringRequest
        {
            JobTitle = request.JobTitle, Description = request.Description,
            Justification = request.Justification, Headcount = request.Headcount,
            BudgetRangeLow = request.BudgetRangeLow, BudgetRangeHigh = request.BudgetRangeHigh,
            EmploymentType = request.EmploymentType,
            RequestedById = user.EmployeeId.Value, DepartmentId = user.Employee!.DepartmentId
        };
        _context.HiringRequests.Add(hr);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetHiringRequests), new { id = hr.Id }, hr);
    }

    [HttpPut("hiring-requests/{id}/approve")]
    public async Task<IActionResult> ApproveHiringRequest(int id, [FromBody] HiringRequestApproval request)
    {
        var hr = await _context.HiringRequests.FindAsync(id);
        if (hr == null) return NotFound();

        var validTransitions = new Dictionary<string, string>
        {
            ["PendingDeptApproval"] = "PendingHrApproval",
            ["PendingHrApproval"] = "PendingBudgetApproval",
            ["PendingBudgetApproval"] = "Approved"
        };
        if (request.Approved)
        {
            if (!validTransitions.TryGetValue(hr.Status, out var nextStatus))
                return BadRequest($"Cannot approve from status '{hr.Status}'");
            if (hr.Status == "PendingDeptApproval") hr.DeptApprovalNotes = request.Notes;
            else if (hr.Status == "PendingHrApproval") hr.HrApprovalNotes = request.Notes;
            else if (hr.Status == "PendingBudgetApproval") hr.BudgetApprovalNotes = request.Notes;
            hr.Status = nextStatus;
        }
        else
        {
            hr.Status = "Rejected";
            hr.DeptApprovalNotes = request.Notes;
        }
        hr.UpdatedAt = DateTime.UtcNow;

        if (hr.Status == "Approved")
        {
            var requisition = new JobRequisition
            {
                Title = hr.JobTitle, Description = hr.Description,
                DepartmentId = hr.DepartmentId, Status = "Open"
            };
            _context.JobRequisitions.Add(requisition);
            await _context.SaveChangesAsync();
            hr.JobRequisitionId = requisition.Id;
        }
        await _context.SaveChangesAsync();
        return Ok(hr);
    }

    // === Offers ===

    [HttpGet("offers")]
    public async Task<IActionResult> GetOffers()
    {
        var offers = await _context.Offers
            .Include(o => o.Candidate)
            .Include(o => o.ApprovedBy)
            .OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(offers.Select(MapOffer));
    }

    [HttpPost("offers")]
    public async Task<IActionResult> CreateOffer([FromBody] CreateOffer request)
    {
        var offer = new Offer
        {
            CandidateId = request.CandidateId, Salary = request.Salary,
            Currency = request.Currency, Benefits = request.Benefits,
            StartDate = request.StartDate, Status = "Draft"
        };
        _context.Offers.Add(offer);
        await _context.SaveChangesAsync();
        return Ok(offer);
    }

    [HttpPut("offers/{id}/approve")]
    public async Task<IActionResult> ApproveOffer(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        var offer = await _context.Offers.Include(o => o.Candidate).FirstOrDefaultAsync(o => o.Id == id);
        if (offer == null) return NotFound();
        offer.Status = "Approved";
        offer.ApprovedById = user?.EmployeeId;
        offer.Candidate.Status = "Offered";
        await _context.SaveChangesAsync();
        return Ok(offer);
    }

    [HttpPut("offers/{id}/accept")]
    public async Task<IActionResult> AcceptOffer(int id)
    {
        var offer = await _context.Offers.Include(o => o.Candidate).FirstOrDefaultAsync(o => o.Id == id);
        if (offer == null) return NotFound();
        offer.Status = "Accepted";
        offer.AcceptedAt = DateTime.UtcNow;
        offer.Candidate.Status = "Hired";
        await _context.SaveChangesAsync();
        return Ok(offer);
    }

    [HttpPut("offers/{id}/reject")]
    public async Task<IActionResult> RejectOffer(int id, [FromBody] RejectOfferRequest request)
    {
        var offer = await _context.Offers.Include(o => o.Candidate).FirstOrDefaultAsync(o => o.Id == id);
        if (offer == null) return NotFound();
        offer.Status = "Rejected";
        offer.RejectedAt = DateTime.UtcNow;
        offer.RejectionReason = request.Reason;
        offer.Candidate.Status = "Rejected";
        await _context.SaveChangesAsync();
        return Ok(offer);
    }

    // === Background Checks ===

    [HttpGet("background-checks/{candidateId}")]
    public async Task<IActionResult> GetBackgroundCheck(int candidateId)
    {
        var bc = await _context.BackgroundChecks.FirstOrDefaultAsync(b => b.CandidateId == candidateId);
        return Ok(bc);
    }

    [HttpPost("background-checks")]
    public async Task<IActionResult> InitiateBackgroundCheck([FromBody] InitiateBackgroundCheck request)
    {
        var bc = new BackgroundCheck { CandidateId = request.CandidateId, VendorName = request.VendorName };
        _context.BackgroundChecks.Add(bc);
        var candidate = await _context.CandidateProfiles.FindAsync(request.CandidateId);
        if (candidate != null) candidate.Status = "BackgroundCheck";
        await _context.SaveChangesAsync();
        return Ok(bc);
    }

    [HttpPut("background-checks/{id}")]
    public async Task<IActionResult> UpdateBackgroundCheck(int id, [FromBody] UpdateBackgroundCheck request)
    {
        var bc = await _context.BackgroundChecks.FindAsync(id);
        if (bc == null) return NotFound();
        bc.Status = request.Status;
        bc.Notes = request.Notes;
        bc.CompletedAt = request.Status == "Cleared" || request.Status == "Failed" ? DateTime.UtcNow : null;
        await _context.SaveChangesAsync();
        return Ok(bc);
    }

    // === Onboarding ===

    [HttpGet("onboarding/{candidateId}")]
    public async Task<IActionResult> GetOnboardingTasks(int candidateId)
    {
        var tasks = await _context.OnboardingTasks.Where(t => t.CandidateId == candidateId).ToListAsync();
        return Ok(tasks);
    }

    [HttpPost("onboarding")]
    public async Task<IActionResult> CreateOnboardingTask([FromBody] CreateOnboardingTask request)
    {
        var task = new OnboardingTask
        {
            CandidateId = request.CandidateId, Title = request.Title,
            Description = request.Description, Category = request.Category,
            AssignedTo = request.AssignedTo
        };
        _context.OnboardingTasks.Add(task);
        await _context.SaveChangesAsync();
        return Ok(task);
    }

    [HttpPut("onboarding/{id}/toggle")]
    public async Task<IActionResult> ToggleOnboardingTask(int id)
    {
        var task = await _context.OnboardingTasks.FindAsync(id);
        if (task == null) return NotFound();
        task.IsCompleted = !task.IsCompleted;
        task.CompletedAt = task.IsCompleted ? DateTime.UtcNow : null;
        await _context.SaveChangesAsync();
        return Ok(task);
    }

    [HttpDelete("onboarding/{id}")]
    public async Task<IActionResult> DeleteOnboardingTask(int id)
    {
        var task = await _context.OnboardingTasks.FindAsync(id);
        if (task == null) return NotFound();
        _context.OnboardingTasks.Remove(task);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // === Dashboard Metrics ===

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var candidates = await _context.CandidateProfiles.ToListAsync();
        var requisitions = await _context.JobRequisitions.ToListAsync();
        var offers = await _context.Offers.ToListAsync();
        var now = DateTime.UtcNow;

        return Ok(new
        {
            TotalCandidates = candidates.Count,
            TotalRequisitions = requisitions.Count,
            OpenRequisitions = requisitions.Count(r => r.Status == "Open"),
            NewCandidates = candidates.Count(c => c.Status == "New"),
            InScreening = candidates.Count(c => c.Status == "Screening"),
            Interviewed = candidates.Count(c => c.Status == "Interviewed"),
            Offered = candidates.Count(c => c.Status == "Offered"),
            Hired = candidates.Count(c => c.Status == "Hired"),
            OfferAcceptanceRate = offers.Any() ? $"{Math.Round((double)offers.Count(o => o.Status == "Accepted") / offers.Count(o => o.Status != "Draft") * 100, 1)}%" : "0%",
            AvgDaysSinceCreated = candidates.Any() ? Math.Round(candidates.Average(c => (now - c.CreatedAt).TotalDays), 1) : 0
        });
    }

    // === Mapping ===

    private static object MapCandidate(CandidateProfile c) => new
    {
        c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.Status, c.Source, c.MatchScore, c.CreatedAt, c.UpdatedAt,
        c.JobRequisitionId, JobTitle = c.JobRequisition?.Title,
        HasResume = !string.IsNullOrWhiteSpace(c.ResumeText),
        ResumePreview = string.IsNullOrWhiteSpace(c.ResumeText) ? null : c.ResumeText.Length > 120 ? c.ResumeText[..120] + "…" : c.ResumeText,
        c.ScreeningSummary,
        Offers = c.Offers?.Select(MapOffer),
        BackgroundCheck = c.BackgroundChecks?.OrderByDescending(b => b.InitiatedAt).FirstOrDefault(),
        OnboardingTasks = c.OnboardingTasks
    };

    private static object MapHiringRequest(HiringRequest h) => new
    {
        h.Id, h.JobTitle, h.Description, h.Justification, h.Headcount, h.BudgetRangeLow, h.BudgetRangeHigh,
        h.EmploymentType, h.Status, h.DeptApprovalNotes, h.HrApprovalNotes, h.BudgetApprovalNotes,
        h.CreatedAt, h.UpdatedAt, h.JobRequisitionId,
        RequestedByName = $"{h.RequestedBy.FirstName} {h.RequestedBy.LastName}",
        DepartmentName = h.Department?.Name
    };

    private static object MapOffer(Offer o) => new
    {
        o.Id, o.Salary, o.Currency, o.Benefits, o.StartDate, o.Status, o.CreatedAt, o.AcceptedAt, o.RejectedAt, o.RejectionReason,
        o.CandidateId, CandidateName = o.Candidate != null ? $"{o.Candidate.FirstName} {o.Candidate.LastName}" : null!,
        ApprovedByName = o.ApprovedBy != null ? $"{o.ApprovedBy.FirstName} {o.ApprovedBy.LastName}" : null!
    };
}

// DTOs
public record CreateCandidate(string FirstName, string LastName, string Email, string? Phone, int? JobRequisitionId, string? Source);
public record CreateJobRequisition(string Title, string? Description, string? Requirements, int? DepartmentId);
public record UpdateJobRequisition(string Title, string? Description, string? Requirements, int? DepartmentId, string Status);
public record CreateInterview(int CandidateId, DateTime ScheduledDate, string? InterviewerName, string? InterviewType);
public record UpdateStatusRequest(string Status);
public record CreateHiringRequest(string JobTitle, string? Description, string? Justification, int Headcount, decimal? BudgetRangeLow, decimal? BudgetRangeHigh, string EmploymentType);
public record HiringRequestApproval(bool Approved, string? Notes);
public record CreateOffer(int CandidateId, decimal Salary, string? Currency, string? Benefits, DateTime? StartDate);
public record RejectOfferRequest(string? Reason);
public record InitiateBackgroundCheck(int CandidateId, string? VendorName);
public record UpdateBackgroundCheck(string Status, string? Notes);
public record CreateOnboardingTask(int CandidateId, string Title, string? Description, string Category, string? AssignedTo);
