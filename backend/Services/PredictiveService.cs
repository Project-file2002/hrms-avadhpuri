using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Predictive;
using HRMS.API.Models.Entities;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Services;

public class PredictiveService : IPredictiveService
{
    private readonly HRMSDbContext _context;

    public PredictiveService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<PredictiveDashboardDto> GetDashboardAsync()
    {
        var attrition = await GetAttritionRisksAsync();
        var hiring = await GetHiringForecastAsync();
        var burnout = await GetBurnoutRisksAsync();

        var total = await _context.Employees.CountAsync(e => !e.IsDeleted);

        return new PredictiveDashboardDto
        {
            AttritionRisks = attrition,
            HiringForecast = hiring,
            BurnoutRisks = burnout,
            Summary = new DashboardSummaryDto
            {
                TotalEmployees = total,
                AtRiskEmployees = attrition.Count(a => a.RiskLevel is "High" or "Critical"),
                HighBurnoutEmployees = burnout.Count(b => b.RiskLevel is "High" or "Critical"),
                OpenPositions = hiring.OpenPositions,
                AvgRiskScore = attrition.Count > 0 ? attrition.Average(a => a.RiskScore) : 0,
                AvgBurnoutScore = burnout.Count > 0 ? burnout.Average(b => b.BurnoutScore) : 0
            }
        };
    }

    public async Task<List<AttritionRiskDto>> GetAttritionRisksAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Where(e => !e.IsDeleted && e.Status != "Resigned" && e.Status != "Terminated")
            .ToListAsync();

        var now = DateTime.UtcNow;
        var ninetyDaysAgo = now.AddDays(-90);
        var risks = new List<AttritionRiskDto>();

        foreach (var emp in employees)
        {
            var factors = new List<string>();
            var score = 0.0;

            // Factor 1: Sick leave spikes in last 90 days
            var sickLeaves = await _context.LeaveRequests
                .Where(lr => lr.EmployeeId == emp.Id && lr.StartDate >= ninetyDaysAgo
                    && lr.LeaveType.Name.Contains("Sick"))
                .CountAsync();

            if (sickLeaves >= 3) { score += 15; factors.Add($"Frequent sick leaves ({sickLeaves} in 90d)"); }
            else if (sickLeaves >= 1) { score += 5; }

            // Factor 2: Absenteeism in last 90 days
            var attendanceRecords = await _context.AttendanceRecords
                .Where(ar => ar.EmployeeId == emp.Id && ar.Date >= ninetyDaysAgo)
                .ToListAsync();

            var absentCount = attendanceRecords.Count(a => a.Status == "Absent");
            var attendanceRate = attendanceRecords.Count > 0
                ? (double)absentCount / attendanceRecords.Count * 100 : 0;

            if (attendanceRate > 20) { score += 25; factors.Add($"High absenteeism ({attendanceRate:F0}% absence rate)"); }
            else if (attendanceRate > 10) { score += 10; factors.Add($"Moderate absenteeism ({attendanceRate:F0}% absence rate)"); }

            // Factor 3: Late arrivals
            var lateCount = attendanceRecords.Count(a => a.Status == "Late");
            if (lateCount >= 5) { score += 10; factors.Add($"Frequent late arrivals ({lateCount} times in 90d)"); }
            else if (lateCount >= 3) { score += 5; }

            // Factor 4: Performance decline
            var reviews = await _context.PerformanceReviews
                .Where(pr => pr.EmployeeId == emp.Id && pr.Status == "Completed")
                .OrderByDescending(pr => pr.EndDate)
                .Take(2)
                .ToListAsync();

            if (reviews.Count >= 2)
            {
                var latest = reviews[0].OverallScore ?? 0;
                var previous = reviews[1].OverallScore ?? 0;
                if (latest < previous - 0.5m)
                {
                    score += 15;
                    factors.Add($"Performance decline ({previous:F1} → {latest:F1})");
                }
            }

            // Factor 5: Short tenure (< 6 months)
            if (emp.DateOfJoining.HasValue)
            {
                var tenureDays = (now - emp.DateOfJoining.Value).TotalDays;
                if (tenureDays < 180) { score += 10; factors.Add("Short tenure (< 6 months)"); }
                else if (tenureDays < 365) { score += 5; }
            }

            // Factor 6: Probation status
            if (emp.Status == "Probation") { score += 15; factors.Add("On probation"); }

            // Cap score at 100
            score = Math.Min(score, 100);

            var level = score < 20 ? "Low" : score < 40 ? "Medium" : score < 65 ? "High" : "Critical";

            risks.Add(new AttritionRiskDto
            {
                EmployeeId = emp.Id,
                EmployeeName = $"{emp.FirstName} {emp.LastName}",
                Department = emp.Department?.Name,
                Position = emp.Position,
                RiskScore = score,
                RiskLevel = level,
                RiskFactors = factors,
                SuggestedAction = level switch
                {
                    "Critical" => "Immediate retention conversation needed. Consider engagement survey.",
                    "High" => "Schedule check-in with manager. Review workload and growth path.",
                    "Medium" => "Monitor engagement. Regular 1:1s recommended.",
                    _ => "No action needed. Risk is low."
                }
            });
        }

        return risks.OrderByDescending(r => r.RiskScore).ToList();
    }

    public async Task<HiringForecastDto> GetHiringForecastAsync()
    {
        var now = DateTime.UtcNow;
        var openJobs = await _context.JobRequisitions
            .Include(jr => jr.Candidates)
            .Include(jr => jr.Department)
            .Where(jr => jr.Status == "Open")
            .ToListAsync();

        var totalCandidates = openJobs.Sum(j => j.Candidates.Count);

        var positions = openJobs.Select(j => new PositionForecastDto
        {
            Title = j.Title,
            Department = j.Department?.Name,
            DaysOpen = (int)(now - j.CreatedAt).TotalDays,
            CandidateCount = j.Candidates.Count,
            Urgency = (now - j.CreatedAt).TotalDays switch
            {
                > 60 => "Critical",
                > 30 => "High",
                > 14 => "Moderate",
                _ => "Normal"
            }
        }).ToList();

        return new HiringForecastDto
        {
            OpenPositions = openJobs.Count,
            TotalCandidates = totalCandidates,
            AvgDaysOpen = positions.Count > 0 ? positions.Average(p => p.DaysOpen) : 0,
            Positions = positions
        };
    }

    public async Task<List<BurnoutRiskDto>> GetBurnoutRisksAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Where(e => !e.IsDeleted && e.Status != "Resigned" && e.Status != "Terminated")
            .ToListAsync();

        var now = DateTime.UtcNow;
        var ninetyDaysAgo = now.AddDays(-90);
        var risks = new List<BurnoutRiskDto>();

        foreach (var emp in employees)
        {
            var indicators = new List<string>();
            var score = 0.0;

            // Factor 1: Late checkouts (overtime indicator)
            var attendanceRecords = await _context.AttendanceRecords
                .Where(ar => ar.EmployeeId == emp.Id && ar.Date >= ninetyDaysAgo && ar.CheckOutTime != null)
                .ToListAsync();

            var lateCheckouts = attendanceRecords
                .Count(a => a.CheckOutTime!.Value.Hour >= 19);

            if (attendanceRecords.Count > 0)
            {
                var latePct = (double)lateCheckouts / attendanceRecords.Count * 100;
                if (latePct > 40) { score += 25; indicators.Add($"Frequent late checkouts ({latePct:F0}% >7PM)"); }
                else if (latePct > 20) { score += 12; indicators.Add($"Moderate late checkouts ({latePct:F0}% >7PM)"); }
            }

            // Factor 2: Early check-ins (starting too early)
            var earlyCheckins = attendanceRecords
                .Count(a => a.CheckInTime.HasValue && a.CheckInTime.Value.Hour < 7);

            if (earlyCheckins > 10) { score += 15; indicators.Add($"Regular early start ({earlyCheckins} days before 7AM)"); }
            else if (earlyCheckins > 5) { score += 8; }

            // Factor 3: No leaves taken (not taking breaks)
            var leavesTaken = await _context.LeaveRequests
                .Where(lr => lr.EmployeeId == emp.Id && lr.StartDate >= ninetyDaysAgo && lr.Status == "Approved")
                .CountAsync();

            if (leavesTaken == 0) { score += 20; indicators.Add("No leave taken in 90 days (no breaks)"); }

            // Factor 4: Frequent present but declining trend
            var monthlyStats = attendanceRecords
                .GroupBy(a => new { a.Date.Year, a.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { Month = g.Key, Present = g.Count(a => a.Status == "Present") })
                .ToList();

            if (monthlyStats.Count >= 2)
            {
                var prevMonth = monthlyStats[^2].Present;
                var currMonth = monthlyStats[^1].Present;
                if (currMonth < prevMonth * 0.7)
                {
                    score += 15;
                    indicators.Add($"Attendance declining (prev month {prevMonth} → {currMonth})");
                }
            }

            // Factor 5: Multiple status changes (erratic)
            if (emp.UpdatedAt.HasValue && (now - emp.UpdatedAt.Value).TotalDays < 30)
            {
                score += 10;
                indicators.Add("Recent profile changes");
            }

            // Cap score
            score = Math.Min(score, 100);
            var level = score < 20 ? "Low" : score < 40 ? "Medium" : score < 65 ? "High" : "Critical";

            risks.Add(new BurnoutRiskDto
            {
                EmployeeId = emp.Id,
                EmployeeName = $"{emp.FirstName} {emp.LastName}",
                Department = emp.Department?.Name,
                BurnoutScore = score,
                RiskLevel = level,
                Indicators = indicators,
                SuggestedAction = level switch
                {
                    "Critical" => "Immediate intervention. Reduce workload, encourage leave, offer counseling.",
                    "High" => "Discuss workload balance. Encourage breaks and time off.",
                    "Medium" => "Monitor hours. Promote work-life balance practices.",
                    _ => "No concern detected."
                }
            });
        }

        return risks.OrderByDescending(r => r.BurnoutScore).ToList();
    }
}
