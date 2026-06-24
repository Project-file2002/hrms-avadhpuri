using HRMS.API.Models.DTOs.Attendance;

namespace HRMS.API.Services.Interfaces;

public interface IAttendanceService
{
    Task<IEnumerable<AttendanceRecordDto>> GetAllAsync(int? employeeId = null, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<AttendanceRecordDto>> GetRecordsAsync(int employeeId, DateTime? from = null, DateTime? to = null);
    Task<AttendanceRecordDto> CreateAsync(int employeeId, CreateAttendanceRecord request);
    Task<IEnumerable<AttendanceCorrectionDto>> GetCorrectionsAsync(int employeeId);
    Task<AttendanceCorrectionDto> RequestCorrectionAsync(int employeeId, CreateAttendanceCorrection request);
    Task<AttendanceRecordDto?> UpdateAsync(int id, CreateAttendanceRecord request);
    Task<bool> DeleteAsync(int id);
}
