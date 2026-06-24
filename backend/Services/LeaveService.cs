using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Leave;
using HRMS.API.Models.Entities;
using HRMS.API.Models.Mapping;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Services;

public class LeaveService : ILeaveService
{
    private readonly HRMSDbContext _context;

    public LeaveService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetAllAsync(int? employeeId = null)
    {
        var query = _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(lr => lr.EmployeeId == employeeId.Value);

        var requests = await query.OrderByDescending(lr => lr.CreatedAt).ToListAsync();
        return requests.Select(lr => lr.ToDto());
    }

    public async Task<LeaveRequestDto?> GetByIdAsync(int id)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .FirstOrDefaultAsync(lr => lr.Id == id);

        return leaveRequest?.ToDto();
    }

    public async Task<LeaveRequestDto> CreateAsync(int employeeId, CreateLeaveRequest request)
    {
        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            Status = "Pending"
        };

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        await _context.Entry(leaveRequest).Reference(lr => lr.Employee).LoadAsync();
        await _context.Entry(leaveRequest).Reference(lr => lr.LeaveType).LoadAsync();

        return leaveRequest.ToDto();
    }

    public async Task<LeaveRequestDto?> UpdateAsync(int id, UpdateLeaveRequest request)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .FirstOrDefaultAsync(lr => lr.Id == id && lr.Status == "Pending");
        if (leaveRequest == null) return null;
        leaveRequest.LeaveTypeId = request.LeaveTypeId;
        leaveRequest.StartDate = request.StartDate;
        leaveRequest.EndDate = request.EndDate;
        leaveRequest.Reason = request.Reason;
        await _context.SaveChangesAsync();
        return leaveRequest.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var leaveRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(lr => lr.Id == id && lr.Status == "Pending");
        if (leaveRequest == null) return false;
        _context.LeaveRequests.Remove(leaveRequest);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<LeaveRequestDto?> ApproveAsync(int id, int reviewerId, ApproveLeaveRequest request)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .FirstOrDefaultAsync(lr => lr.Id == id);

        if (leaveRequest == null) return null;

        leaveRequest.Status = request.Status;
        leaveRequest.ReviewNotes = request.ReviewNotes;
        leaveRequest.ReviewedBy = reviewerId;
        leaveRequest.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return leaveRequest.ToDto();
    }

    public async Task<IEnumerable<LeaveTypeDto>> GetLeaveTypesAsync()
    {
        var types = await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();
        return types.Select(lt => lt.ToDto());
    }

    public async Task<IEnumerable<LeaveBalanceDto>> GetLeaveBalancesAsync(int employeeId)
    {
        var balances = await _context.LeaveBalances
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == DateTime.UtcNow.Year)
            .ToListAsync();

        return balances.Select(lb => lb.ToDto());
    }
}
