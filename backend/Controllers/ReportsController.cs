using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager,Manager,PayrollStaff")]
public class ReportsController : ControllerBase
{
    private readonly HRMSDbContext _context;

    public ReportsController(HRMSDbContext context)
    {
        _context = context;
    }

    [HttpGet("headcount")]
    public async Task<IActionResult> GetHeadcount()
    {
        var total = await _context.Employees.CountAsync(e => !e.IsDeleted);
        var byDepartment = await _context.Employees
            .Where(e => !e.IsDeleted && e.Department != null)
            .GroupBy(e => e.Department!.Name)
            .Select(g => new { department = g.Key, count = g.Count() })
            .ToListAsync();
        var byStatus = await _context.Employees
            .Where(e => !e.IsDeleted)
            .GroupBy(e => e.Status)
            .Select(g => new { status = g.Key, count = g.Count() })
            .ToListAsync();

        return Ok(new { total, byDepartment, byStatus });
    }

    [HttpGet("leave-summary")]
    public async Task<IActionResult> GetLeaveSummary()
    {
        var total = await _context.LeaveRequests.CountAsync();
        var pending = await _context.LeaveRequests.CountAsync(lr => lr.Status == "Pending");
        var approved = await _context.LeaveRequests.CountAsync(lr => lr.Status == "Approved");
        var rejected = await _context.LeaveRequests.CountAsync(lr => lr.Status == "Rejected");
        var byType = await _context.LeaveRequests
            .GroupBy(lr => lr.LeaveType.Name)
            .Select(g => new { type = g.Key, count = g.Count() })
            .ToListAsync();

        return Ok(new { total, pending, approved, rejected, byType });
    }

    [HttpGet("attendance-summary")]
    public async Task<IActionResult> GetAttendanceSummary()
    {
        var today = DateTime.UtcNow.Date;
        var todayRecords = await _context.AttendanceRecords
            .Where(ar => ar.Date == today)
            .CountAsync();
        var present = await _context.AttendanceRecords
            .Where(ar => ar.Date == today && ar.Status == "Present")
            .CountAsync();
        var late = await _context.AttendanceRecords
            .Where(ar => ar.Date == today && ar.Status == "Late")
            .CountAsync();

        return Ok(new { date = today.ToShortDateString(), todayRecords, present, late });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var employeeCount = await _context.Employees.CountAsync(e => !e.IsDeleted);
        var pendingLeaves = await _context.LeaveRequests.CountAsync(lr => lr.Status == "Pending");
        var openPositions = await _context.JobRequisitions.CountAsync(jr => jr.Status == "Open");
        var todayDate = DateTime.UtcNow.Date;
        var todayAttendance = await _context.AttendanceRecords
            .Where(ar => ar.Date == todayDate)
            .CountAsync();

        return Ok(new
        {
            totalEmployees = employeeCount,
            pendingLeaves,
            openPositions,
            todayAttendance,
        });
    }
}
