using HRMS.API.Models.DTOs.Leave;

namespace HRMS.API.Services.Interfaces;

public interface ILeaveService
{
    Task<IEnumerable<LeaveRequestDto>> GetAllAsync(int? employeeId = null);
    Task<LeaveRequestDto?> GetByIdAsync(int id);
    Task<LeaveRequestDto> CreateAsync(int employeeId, CreateLeaveRequest request);
    Task<LeaveRequestDto?> UpdateAsync(int id, UpdateLeaveRequest request);
    Task<bool> DeleteAsync(int id);
    Task<LeaveRequestDto?> ApproveAsync(int id, int reviewerId, ApproveLeaveRequest request);
    Task<IEnumerable<LeaveTypeDto>> GetLeaveTypesAsync();
    Task<IEnumerable<LeaveBalanceDto>> GetLeaveBalancesAsync(int employeeId);
}
