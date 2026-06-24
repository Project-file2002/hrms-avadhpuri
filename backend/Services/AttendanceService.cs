using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Attendance;
using HRMS.API.Models.Entities;
using HRMS.API.Models.Mapping;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Services;

public class AttendanceService : IAttendanceService
{
    private readonly HRMSDbContext _context;

    public AttendanceService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AttendanceRecordDto>> GetAllAsync(int? employeeId = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.AttendanceRecords
            .Include(ar => ar.Employee)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(ar => ar.EmployeeId == employeeId.Value);
        if (from.HasValue) query = query.Where(ar => ar.Date >= from.Value);
        if (to.HasValue) query = query.Where(ar => ar.Date <= to.Value);

        var records = await query.OrderByDescending(ar => ar.Date).ToListAsync();
        return records.Select(ar => ar.ToDto());
    }

    public async Task<IEnumerable<AttendanceRecordDto>> GetRecordsAsync(int employeeId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.AttendanceRecords
            .Include(ar => ar.Employee)
            .Where(ar => ar.EmployeeId == employeeId)
            .AsQueryable();

        if (from.HasValue) query = query.Where(ar => ar.Date >= from.Value);
        if (to.HasValue) query = query.Where(ar => ar.Date <= to.Value);

        var records = await query.OrderByDescending(ar => ar.Date).ToListAsync();
        return records.Select(ar => ar.ToDto());
    }

    public async Task<AttendanceRecordDto> CreateAsync(int employeeId, CreateAttendanceRecord request)
    {
        var record = new AttendanceRecord
        {
            EmployeeId = employeeId,
            Date = request.Date,
            CheckInTime = request.CheckInTime,
            CheckOutTime = request.CheckOutTime
        };

        _context.AttendanceRecords.Add(record);
        await _context.SaveChangesAsync();

        return record.ToDto();
    }

    public async Task<IEnumerable<AttendanceCorrectionDto>> GetCorrectionsAsync(int employeeId)
    {
        var corrections = await _context.AttendanceCorrections
            .Where(ac => ac.EmployeeId == employeeId)
            .OrderByDescending(ac => ac.CreatedAt)
            .ToListAsync();

        return corrections.Select(ac => ac.ToDto());
    }

    public async Task<AttendanceRecordDto?> UpdateAsync(int id, CreateAttendanceRecord request)
    {
        var record = await _context.AttendanceRecords.FindAsync(id);
        if (record == null) return null;
        record.Date = request.Date;
        record.CheckInTime = request.CheckInTime;
        record.CheckOutTime = request.CheckOutTime;
        await _context.SaveChangesAsync();
        return record.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var record = await _context.AttendanceRecords.FindAsync(id);
        if (record == null) return false;
        _context.AttendanceRecords.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AttendanceCorrectionDto> RequestCorrectionAsync(int employeeId, CreateAttendanceCorrection request)
    {
        var correction = new AttendanceCorrection
        {
            EmployeeId = employeeId,
            Date = request.Date,
            CorrectedCheckIn = request.CorrectedCheckIn,
            CorrectedCheckOut = request.CorrectedCheckOut,
            Reason = request.Reason
        };

        _context.AttendanceCorrections.Add(correction);
        await _context.SaveChangesAsync();

        return correction.ToDto();
    }
}
