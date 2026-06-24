using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Authorization;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Attendance;
using HRMS.API.Models.Entities;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly HRMSDbContext _context;

    public AttendanceController(IAttendanceService attendanceService, HRMSDbContext context)
    {
        _attendanceService = attendanceService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? employeeId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var user = await GetCurrentUserAsync();
        var (scopeEmployeeId, access) = HrAccess.ResolveEmployeeScope(User, user, employeeId);
        if (access == HrAccessResult.Unauthorized) return Unauthorized();
        if (access == HrAccessResult.Forbidden) return Forbid();

        var records = await _attendanceService.GetAllAsync(scopeEmployeeId, from, to);
        return Ok(records);
    }

    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetRecords(int employeeId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var user = await GetCurrentUserAsync();
        var (scopeEmployeeId, access) = HrAccess.ResolveEmployeeScope(User, user, employeeId);
        if (access == HrAccessResult.Unauthorized) return Unauthorized();
        if (access == HrAccessResult.Forbidden) return Forbid();

        var records = await _attendanceService.GetRecordsAsync(scopeEmployeeId!.Value, from, to);
        return Ok(records);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAttendanceRecord request)
    {
        var user = await GetCurrentUserAsync();
        if (user?.EmployeeId == null) return Unauthorized();

        var record = await _attendanceService.CreateAsync(user.EmployeeId.Value, request);
        return CreatedAtAction(nameof(GetRecords), new { employeeId = user.EmployeeId.Value }, record);
    }

    [HttpGet("corrections/{employeeId:int}")]
    public async Task<IActionResult> GetCorrections(int employeeId)
    {
        var user = await GetCurrentUserAsync();
        var (scopeEmployeeId, access) = HrAccess.ResolveEmployeeScope(User, user, employeeId);
        if (access == HrAccessResult.Unauthorized) return Unauthorized();
        if (access == HrAccessResult.Forbidden) return Forbid();

        var corrections = await _attendanceService.GetCorrectionsAsync(scopeEmployeeId!.Value);
        return Ok(corrections);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateAttendanceRecord request)
    {
        if (!HrAccess.CanViewAllHrData(User)) return Forbid();

        var result = await _attendanceService.UpdateAsync(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!HrAccess.CanViewAllHrData(User)) return Forbid();

        var result = await _attendanceService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("corrections")]
    public async Task<IActionResult> RequestCorrection([FromBody] CreateAttendanceCorrection request)
    {
        var user = await GetCurrentUserAsync();
        if (user?.EmployeeId == null) return Unauthorized();

        var correction = await _attendanceService.RequestCorrectionAsync(user.EmployeeId.Value, request);
        return Ok(correction);
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
}
