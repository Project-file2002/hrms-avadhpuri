using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager,Manager")]
public class WorkflowController : ControllerBase
{
    private readonly HRMSDbContext _context;

    public WorkflowController(HRMSDbContext context) => _context = context;

    // ========== PROMOTIONS ==========

    [HttpGet("promotions")]
    public async Task<IActionResult> GetPromotions()
    {
        var list = await _context.PromotionRequests
            .Include(p => p.Employee).Include(p => p.RequestedBy)
            .Include(p => p.ApprovedByManager).Include(p => p.ApprovedByHrbp)
            .Include(p => p.ApprovedByDeptHead).Include(p => p.ApprovedByCeo)
            .OrderByDescending(p => p.CreatedAt).ToListAsync();
        return Ok(list.Select(MapPromotion));
    }

    [HttpPost("promotions")]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var promotion = new PromotionRequest
        {
            EmployeeId = request.EmployeeId,
            CurrentPosition = request.CurrentPosition,
            CurrentSalary = request.CurrentSalary,
            ProposedPosition = request.ProposedPosition,
            ProposedSalary = request.ProposedSalary,
            Justification = request.Justification,
            RequestedById = user.EmployeeId.Value,
            Status = "PendingManagerApproval"
        };
        _context.PromotionRequests.Add(promotion);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPromotions), new { id = promotion.Id }, promotion);
    }

    [HttpPut("promotions/{id}/approve")]
    public async Task<IActionResult> ApprovePromotion(int id, [FromBody] WorkflowApproval request)
    {
        var p = await _context.PromotionRequests.FindAsync(id);
        if (p == null) return NotFound();

        var transitions = new Dictionary<string, string>
        {
            ["PendingManagerApproval"] = "PendingHrbpApproval",
            ["PendingHrbpApproval"] = "PendingDeptHeadApproval",
            ["PendingDeptHeadApproval"] = "PendingCeoApproval",
            ["PendingCeoApproval"] = "Approved"
        };

        if (!transitions.TryGetValue(p.Status, out var next))
            return BadRequest($"Cannot approve from '{p.Status}'");

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        var empId = user?.EmployeeId;

        if (request.Approved)
        {
            if (p.Status == "PendingManagerApproval") { p.ManagerNotes = request.Notes; p.ApprovedByManagerId = empId; }
            else if (p.Status == "PendingHrbpApproval") { p.HrbpNotes = request.Notes; p.ApprovedByHrbpId = empId; }
            else if (p.Status == "PendingDeptHeadApproval") { p.DeptHeadNotes = request.Notes; p.ApprovedByDeptHeadId = empId; }
            else if (p.Status == "PendingCeoApproval") { p.CeoNotes = request.Notes; p.ApprovedByCeoId = empId; }
            p.Status = next;
        }
        else
        {
            p.Status = "Rejected";
            if (p.Status == "PendingManagerApproval") p.ManagerNotes = request.Notes;
            else if (p.Status == "PendingHrbpApproval") p.HrbpNotes = request.Notes;
            else if (p.Status == "PendingDeptHeadApproval") p.DeptHeadNotes = request.Notes;
            else if (p.Status == "PendingCeoApproval") p.CeoNotes = request.Notes;
        }

        if (p.Status == "Approved")
        {
            var employee = await _context.Employees.FindAsync(p.EmployeeId);
            if (employee != null)
            {
                employee.Position = p.ProposedPosition;
                p.PayrollUpdated = true;
            }
        }

        p.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(p);
    }

    // ========== TRANSFERS ==========

    [HttpGet("transfers")]
    public async Task<IActionResult> GetTransfers()
    {
        var list = await _context.TransferRequests
            .Include(t => t.Employee).Include(t => t.RequestedBy)
            .Include(t => t.CurrentDepartment).Include(t => t.ProposedDepartment)
            .Include(t => t.ApprovedByManager).Include(t => t.ApprovedByHr)
            .Include(t => t.ApprovedByDept).Include(t => t.ApprovedByIt)
            .Include(t => t.ApprovedByPayroll)
            .OrderByDescending(t => t.CreatedAt).ToListAsync();
        return Ok(list.Select(MapTransfer));
    }

    [HttpPost("transfers")]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();
        var emp = await _context.Employees.FindAsync(request.EmployeeId);
        if (emp == null) return NotFound("Employee not found");

        var transfer = new TransferRequest
        {
            EmployeeId = request.EmployeeId,
            CurrentPosition = emp.Position ?? emp.FirstName,
            ProposedPosition = request.ProposedPosition,
            Reason = request.Reason,
            RequestedById = user.EmployeeId.Value,
            CurrentDepartmentId = emp.DepartmentId ?? 1,
            ProposedDepartmentId = request.ProposedDepartmentId,
            Status = "PendingManagerApproval"
        };
        _context.TransferRequests.Add(transfer);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTransfers), new { id = transfer.Id }, transfer);
    }

    [HttpPut("transfers/{id}/approve")]
    public async Task<IActionResult> ApproveTransfer(int id, [FromBody] WorkflowApproval request)
    {
        var t = await _context.TransferRequests.FindAsync(id);
        if (t == null) return NotFound();

        var transitions = new Dictionary<string, string>
        {
            ["PendingManagerApproval"] = "PendingHrApproval",
            ["PendingHrApproval"] = "PendingDepartmentApproval",
            ["PendingDepartmentApproval"] = "PendingItApproval",
            ["PendingItApproval"] = "PendingPayrollApproval",
            ["PendingPayrollApproval"] = "PendingEmployeeAcceptance",
            ["PendingEmployeeAcceptance"] = "Completed"
        };

        if (!transitions.TryGetValue(t.Status, out var next))
            return BadRequest($"Cannot approve from '{t.Status}'");

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        var empId = user?.EmployeeId;

        if (request.Approved)
        {
            if (t.Status == "PendingManagerApproval") { t.ManagerNotes = request.Notes; t.ApprovedByManagerId = empId; }
            else if (t.Status == "PendingHrApproval") { t.HrNotes = request.Notes; t.ApprovedByHrId = empId; }
            else if (t.Status == "PendingDepartmentApproval") { t.DeptNotes = request.Notes; t.ApprovedByDeptId = empId; }
            else if (t.Status == "PendingItApproval") { t.ItNotes = request.Notes; t.ApprovedByItId = empId; }
            else if (t.Status == "PendingPayrollApproval") { t.PayrollNotes = request.Notes; t.ApprovedByPayrollId = empId; }
            else if (t.Status == "PendingEmployeeAcceptance") { t.EmployeeAccepted = true; t.EmployeeNotes = request.Notes; }

            if (next == "Completed" || (t.Status == "PendingEmployeeAcceptance" && request.Approved))
            {
                var employee = await _context.Employees.FindAsync(t.EmployeeId);
                if (employee != null)
                {
                    employee.DepartmentId = t.ProposedDepartmentId;
                    employee.Position = t.ProposedPosition;
                }
            }
            t.Status = next;
        }
        else
        {
            t.Status = "Rejected";
            if (t.Status == "PendingManagerApproval") t.ManagerNotes = request.Notes;
            else if (t.Status == "PendingHrApproval") t.HrNotes = request.Notes;
            else if (t.Status == "PendingDepartmentApproval") t.DeptNotes = request.Notes;
            else if (t.Status == "PendingItApproval") t.ItNotes = request.Notes;
            else if (t.Status == "PendingPayrollApproval") t.PayrollNotes = request.Notes;
            else if (t.Status == "PendingEmployeeAcceptance") t.EmployeeNotes = request.Notes;
        }

        t.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(t);
    }

    // ========== MAPPING ==========

    private static object MapPromotion(PromotionRequest p) => new
    {
        p.Id, p.CurrentPosition, p.CurrentSalary, p.ProposedPosition, p.ProposedSalary,
        p.Justification, p.Status, p.CreatedAt, p.UpdatedAt, p.PayrollUpdated,
        p.ManagerNotes, p.HrbpNotes, p.DeptHeadNotes, p.CeoNotes,
        EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
        RequestedByName = $"{p.RequestedBy.FirstName} {p.RequestedBy.LastName}",
        ApprovedByManagerName = p.ApprovedByManager != null ? $"{p.ApprovedByManager.FirstName} {p.ApprovedByManager.LastName}" : null,
        ApprovedByHrbpName = p.ApprovedByHrbp != null ? $"{p.ApprovedByHrbp.FirstName} {p.ApprovedByHrbp.LastName}" : null,
        ApprovedByDeptHeadName = p.ApprovedByDeptHead != null ? $"{p.ApprovedByDeptHead.FirstName} {p.ApprovedByDeptHead.LastName}" : null,
        ApprovedByCeoName = p.ApprovedByCeo != null ? $"{p.ApprovedByCeo.FirstName} {p.ApprovedByCeo.LastName}" : null,
    };

    private static object MapTransfer(TransferRequest t) => new
    {
        t.Id, t.CurrentPosition, t.ProposedPosition, t.Reason, t.Status, t.CreatedAt, t.UpdatedAt,
        t.EmployeeAccepted, t.ManagerNotes, t.HrNotes, t.DeptNotes, t.ItNotes, t.PayrollNotes, t.EmployeeNotes,
        EmployeeName = $"{t.Employee.FirstName} {t.Employee.LastName}",
        RequestedByName = $"{t.RequestedBy.FirstName} {t.RequestedBy.LastName}",
        CurrentDepartmentName = t.CurrentDepartment.Name,
        ProposedDepartmentName = t.ProposedDepartment.Name,
        ApprovedByManagerName = t.ApprovedByManager != null ? $"{t.ApprovedByManager.FirstName} {t.ApprovedByManager.LastName}" : null,
        ApprovedByHrName = t.ApprovedByHr != null ? $"{t.ApprovedByHr.FirstName} {t.ApprovedByHr.LastName}" : null,
        ApprovedByDeptName = t.ApprovedByDept != null ? $"{t.ApprovedByDept.FirstName} {t.ApprovedByDept.LastName}" : null,
        ApprovedByItName = t.ApprovedByIt != null ? $"{t.ApprovedByIt.FirstName} {t.ApprovedByIt.LastName}" : null,
        ApprovedByPayrollName = t.ApprovedByPayroll != null ? $"{t.ApprovedByPayroll.FirstName} {t.ApprovedByPayroll.LastName}" : null,
    };
}

public record CreatePromotionRequest(int EmployeeId, string CurrentPosition, decimal CurrentSalary, string ProposedPosition, decimal ProposedSalary, string? Justification);
public record CreateTransferRequest(int EmployeeId, string ProposedPosition, string? Reason, int ProposedDepartmentId);
public record WorkflowApproval(bool Approved, string? Notes);
