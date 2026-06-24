using HRMS.API.Models.DTOs.Auth;
using HRMS.API.Models.DTOs.Employee;
using HRMS.API.Models.DTOs.Department;
using HRMS.API.Models.DTOs.Leave;
using HRMS.API.Models.DTOs.Attendance;
using HRMS.API.Models.DTOs.Performance;
using HRMS.API.Models.DTOs.Payroll;
using HRMS.API.Models.DTOs.Recruitment;
using HRMS.API.Models.Entities;

namespace HRMS.API.Models.Mapping;

public static class Mappings
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EmployeeId = user.EmployeeId,
            Roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new()
        };
    }

    public static EmployeeDto ToDto(this Employee emp)
    {
        return new EmployeeDto
        {
            Id = emp.Id,
            EmployeeCode = emp.EmployeeCode,
            FirstName = emp.FirstName,
            LastName = emp.LastName,
            Email = emp.Email,
            Phone = emp.Phone,
            Position = emp.Position,
            DateOfBirth = emp.DateOfBirth,
            DateOfJoining = emp.DateOfJoining,
            Status = emp.Status,
            Gender = emp.Gender,
            Address = emp.Address,
            EmergencyContactName = emp.EmergencyContactName,
            EmergencyContactPhone = emp.EmergencyContactPhone,
            DepartmentName = emp.Department?.Name,
            ManagerName = emp.Manager != null ? $"{emp.Manager.FirstName} {emp.Manager.LastName}" : null
        };
    }

    public static DepartmentDto ToDto(this Department dept)
    {
        return new DepartmentDto
        {
            Id = dept.Id,
            Name = dept.Name,
            Description = dept.Description,
            HeadName = dept.Head != null ? $"{dept.Head.FirstName} {dept.Head.LastName}" : null,
            EmployeeCount = dept.Employees?.Count(e => !e.IsDeleted) ?? 0
        };
    }

    public static LeaveRequestDto ToDto(this LeaveRequest lr)
    {
        return new LeaveRequestDto
        {
            Id = lr.Id,
            EmployeeId = lr.EmployeeId,
            StartDate = lr.StartDate,
            EndDate = lr.EndDate,
            Reason = lr.Reason,
            Status = lr.Status,
            CreatedAt = lr.CreatedAt,
            EmployeeName = $"{lr.Employee.FirstName} {lr.Employee.LastName}",
            LeaveTypeName = lr.LeaveType.Name,
            LeaveTypeId = lr.LeaveTypeId
        };
    }

    public static LeaveTypeDto ToDto(this LeaveType lt)
    {
        return new LeaveTypeDto
        {
            Id = lt.Id,
            Name = lt.Name,
            Description = lt.Description,
            DefaultDays = lt.DefaultDays
        };
    }

    public static LeaveBalanceDto ToDto(this LeaveBalance lb)
    {
        return new LeaveBalanceDto
        {
            LeaveTypeName = lb.LeaveType.Name,
            TotalDays = lb.TotalDays,
            UsedDays = lb.UsedDays
        };
    }

    public static AttendanceRecordDto ToDto(this AttendanceRecord ar)
    {
        return new AttendanceRecordDto
        {
            Id = ar.Id,
            EmployeeId = ar.EmployeeId,
            Date = ar.Date,
            CheckInTime = ar.CheckInTime,
            CheckOutTime = ar.CheckOutTime,
            Status = ar.Status,
            EmployeeName = ar.Employee != null ? $"{ar.Employee.FirstName} {ar.Employee.LastName}" : null
        };
    }

    public static AttendanceCorrectionDto ToDto(this AttendanceCorrection ac)
    {
        return new AttendanceCorrectionDto
        {
            Id = ac.Id,
            Date = ac.Date,
            CorrectedCheckIn = ac.CorrectedCheckIn,
            CorrectedCheckOut = ac.CorrectedCheckOut,
            Reason = ac.Reason,
            Status = ac.Status
        };
    }

    public static PerformanceReviewDto ToDto(this PerformanceReview pr)
    {
        return new PerformanceReviewDto
        {
            Id = pr.Id,
            Title = pr.Title,
            Status = pr.Status,
            Comments = pr.Comments,
            OverallScore = pr.OverallScore,
            EmployeeId = pr.EmployeeId,
            EmployeeName = $"{pr.Employee.FirstName} {pr.Employee.LastName}",
            ReviewerName = $"{pr.Reviewer.FirstName} {pr.Reviewer.LastName}",
            CycleId = pr.CycleId,
            CycleName = pr.Cycle.Name,
            Scores = pr.Scores.Select(s => s.ToDto()).ToList()
        };
    }

    public static ReviewScoreDto ToDto(this ReviewScore rs)
    {
        return new ReviewScoreDto
        {
            Criteria = rs.Criteria,
            Score = rs.Score,
            Comments = rs.Comments
        };
    }

    public static ReviewCycleDto ToDto(this ReviewCycle rc)
    {
        return new ReviewCycleDto
        {
            Id = rc.Id,
            Name = rc.Name,
            StartDate = rc.StartDate,
            EndDate = rc.EndDate,
            Status = rc.Status
        };
    }

    public static PayrollStructureDto ToDto(this PayrollStructure ps)
    {
        return new PayrollStructureDto
        {
            Id = ps.Id,
            Name = ps.Name,
            Description = ps.Description,
            Components = ps.Components.Select(c => c.ToDto()).ToList()
        };
    }

    public static SalaryComponentDto ToDto(this SalaryComponent sc)
    {
        return new SalaryComponentDto
        {
            Name = sc.Name,
            Type = sc.Type,
            Amount = sc.Amount
        };
    }

    public static CandidateDto ToDto(this CandidateProfile cp)
    {
        return new CandidateDto
        {
            Id = cp.Id,
            FirstName = cp.FirstName,
            LastName = cp.LastName,
            Email = cp.Email,
            Phone = cp.Phone,
            Status = cp.Status,
            MatchScore = cp.MatchScore,
            CreatedAt = cp.CreatedAt,
            JobTitle = cp.JobRequisition?.Title
        };
    }

    public static JobRequisitionDto ToDto(this JobRequisition jr)
    {
        return new JobRequisitionDto
        {
            Id = jr.Id,
            Title = jr.Title,
            Description = jr.Description,
            Requirements = jr.Requirements,
            Status = jr.Status,
            CandidateCount = jr.Candidates?.Count ?? 0
        };
    }

    public static InterviewDto ToDto(this InterviewSchedule interview)
    {
        return new InterviewDto
        {
            Id = interview.Id,
            ScheduledDate = interview.ScheduledDate,
            InterviewerName = interview.InterviewerName,
            InterviewType = interview.InterviewType,
            Status = interview.Status,
            Feedback = interview.Feedback,
            Rating = interview.Rating
        };
    }
}
