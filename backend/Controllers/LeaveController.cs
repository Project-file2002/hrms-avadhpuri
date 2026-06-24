using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Leave;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;
    private readonly HRMSDbContext _context;

    public LeaveController(ILeaveService leaveService, HRMSDbContext context)
    {
        _leaveService = leaveService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? employeeId)
    {
        var requests = await _leaveService.GetAllAsync(employeeId);
        return Ok(requests);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var request = await _leaveService.GetByIdAsync(id);
        if (request == null) return NotFound();
        return Ok(request);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var leaveRequest = await _leaveService.CreateAsync(user.EmployeeId.Value, request);
        return CreatedAtAction(nameof(GetById), new { id = leaveRequest.Id }, leaveRequest);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLeaveRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest == null) return NotFound();
        if (leaveRequest.Status != "Pending") return BadRequest("Can only edit pending requests");

        var isAdmin = User.IsInRole("Administrator") || User.IsInRole("HRManager");
        if (leaveRequest.EmployeeId != user.EmployeeId && !isAdmin) return Forbid();

        var result = await _leaveService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest == null) return NotFound();
        if (leaveRequest.Status != "Pending") return BadRequest("Can only cancel pending requests");

        var isAdmin = User.IsInRole("Administrator") || User.IsInRole("HRManager");
        if (leaveRequest.EmployeeId != user.EmployeeId && !isAdmin) return Forbid();

        var result = await _leaveService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [Authorize(Roles = "Administrator,HRManager,Manager")]
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveLeaveRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var leaveRequest = await _leaveService.ApproveAsync(id, userId, request);
        if (leaveRequest == null) return NotFound();
        return Ok(leaveRequest);
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var types = await _leaveService.GetLeaveTypesAsync();
        return Ok(types);
    }

    [HttpGet("balances/{employeeId}")]
    public async Task<IActionResult> GetBalances(int employeeId)
    {
        var balances = await _leaveService.GetLeaveBalancesAsync(employeeId);
        return Ok(balances);
    }
}
