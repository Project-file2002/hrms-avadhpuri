using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Leave;
using HRMS.API.Models.Entities;
using HRMS.API.Services;

namespace HRMS.API.Tests.Services;

public class LeaveServiceTests
{
    private static HRMSDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HRMSDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new HRMSDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_CreatesPendingLeave()
    {
        var context = CreateContext();
        var employee = new Employee { EmployeeCode = "EMP010", FirstName = "Leave", LastName = "User", Email = "leave@test.com" };
        var leaveType = new LeaveType { Name = "Annual", DefaultDays = 15, IsActive = true };
        context.Employees.Add(employee);
        context.LeaveTypes.Add(leaveType);
        await context.SaveChangesAsync();

        var service = new LeaveService(context);
        var result = await service.CreateAsync(employee.Id, new CreateLeaveRequest
        {
            LeaveTypeId = leaveType.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(3),
            Reason = "Vacation"
        });

        Assert.Equal("Pending", result.Status);
        Assert.Equal("Vacation", result.Reason);
    }

    [Fact]
    public async Task ApproveAsync_UpdatesStatus()
    {
        var context = CreateContext();
        var employee = new Employee { EmployeeCode = "EMP011", FirstName = "Approve", LastName = "Test", Email = "approve@test.com" };
        var leaveType = new LeaveType { Name = "Sick", DefaultDays = 10, IsActive = true };
        context.Employees.Add(employee);
        context.LeaveTypes.Add(leaveType);
        await context.SaveChangesAsync();

        var leave = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveTypeId = leaveType.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Reason = "Sick",
            Status = "Pending"
        };
        context.LeaveRequests.Add(leave);
        await context.SaveChangesAsync();

        var service = new LeaveService(context);
        var result = await service.ApproveAsync(leave.Id, employee.Id, new ApproveLeaveRequest
        {
            Status = "Approved",
            ReviewNotes = "Approved"
        });

        Assert.NotNull(result);
        Assert.Equal("Approved", result!.Status);
    }
}
