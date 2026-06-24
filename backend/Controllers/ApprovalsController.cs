using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager,Manager,PayrollStaff")]
public class ApprovalsController : ControllerBase
{
    private readonly HRMSDbContext _context;
    public ApprovalsController(HRMSDbContext context) => _context = context;

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var leave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Where(l => l.Status == "Pending")
            .Select(l => new
            {
                id = l.Id,
                type = "Leave",
                title = $"{l.Employee.FirstName} {l.Employee.LastName} — Leave Request",
                subtitle = l.Reason,
                status = l.Status,
                createdAt = l.CreatedAt,
                link = "/leave"
            }).ToListAsync();

        var expense = await _context.ExpenseReports
            .Include(e => e.Employee)
            .Where(e => e.Status == "Pending")
            .Select(e => new
            {
                id = e.Id,
                type = "Expense",
                title = e.Title,
                subtitle = e.Employee.FirstName + " " + e.Employee.LastName,
                status = e.Status,
                createdAt = e.CreatedAt,
                link = "/expense"
            }).ToListAsync();

        var promotion = await _context.PromotionRequests
            .Include(p => p.Employee)
            .Where(p => !p.Status.Contains("Rejected") && !p.Status.Contains("Completed") && p.Status != "PayrollUpdated")
            .Select(p => new
            {
                id = p.Id,
                type = "Promotion",
                title = $"Promotion — {p.Employee.FirstName} {p.Employee.LastName}",
                subtitle = p.ProposedPosition,
                status = p.Status,
                createdAt = p.CreatedAt,
                link = "/career-workflows"
            }).ToListAsync();

        var transfer = await _context.TransferRequests
            .Include(t => t.Employee)
            .Where(t => t.Status != "Completed" && t.Status != "Rejected" && !t.Status.Contains("Rejected"))
            .Select(t => new
            {
                id = t.Id,
                type = "Transfer",
                title = $"Transfer — {t.Employee.FirstName} {t.Employee.LastName}",
                subtitle = t.Reason,
                status = t.Status,
                createdAt = t.CreatedAt,
                link = "/career-workflows"
            }).ToListAsync();

        var hiring = await _context.HiringRequests
            .Include(h => h.Department)
            .Where(h => h.Status != "Approved" && h.Status != "Rejected" && !h.Status.Contains("Rejected"))
            .Select(h => new
            {
                id = h.Id,
                type = "Recruitment",
                title = h.JobTitle,
                subtitle = h.Department != null ? h.Department.Name : "",
                status = h.Status,
                createdAt = h.CreatedAt,
                link = "/recruitment"
            }).ToListAsync();

        var all = leave.Cast<object>()
            .Concat(expense)
            .Concat(promotion)
            .Concat(transfer)
            .Concat(hiring)
            .OrderByDescending(x => ((dynamic)x).createdAt)
            .ToList();

        return Ok(new
        {
            total = all.Count,
            items = all,
            summary = new
            {
                leave = leave.Count,
                expense = expense.Count,
                promotion = promotion.Count,
                transfer = transfer.Count,
                recruitment = hiring.Count
            }
        });
    }
}
