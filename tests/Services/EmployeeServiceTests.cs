using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Employee;
using HRMS.API.Models.Entities;
using HRMS.API.Services;

namespace HRMS.API.Tests.Services;

public class EmployeeServiceTests
{
    private static HRMSDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HRMSDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new HRMSDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyNonDeleted()
    {
        var context = CreateContext();
        context.Employees.AddRange(
            new Employee { EmployeeCode = "EMP001", FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new Employee { EmployeeCode = "EMP002", FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", IsDeleted = true }
        );
        await context.SaveChangesAsync();

        var service = new EmployeeService(context);
        var result = await service.GetAllAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task CreateAsync_AddsEmployee()
    {
        var context = CreateContext();
        var service = new EmployeeService(context);

        var request = new CreateEmployeeRequest
        {
            EmployeeCode = "EMP003",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com"
        };

        var result = await service.CreateAsync(request);

        Assert.Equal("EMP003", result.EmployeeCode);
        Assert.Equal("Test", result.FirstName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenDeleted()
    {
        var context = CreateContext();
        var emp = new Employee { EmployeeCode = "EMP004", FirstName = "Ghost", LastName = "User", Email = "ghost@test.com", IsDeleted = true };
        context.Employees.Add(emp);
        await context.SaveChangesAsync();

        var service = new EmployeeService(context);
        var result = await service.GetByIdAsync(emp.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletes()
    {
        var context = CreateContext();
        var emp = new Employee { EmployeeCode = "EMP005", FirstName = "Delete", LastName = "Me", Email = "delete@test.com" };
        context.Employees.Add(emp);
        await context.SaveChangesAsync();

        var service = new EmployeeService(context);
        var result = await service.DeleteAsync(emp.Id);

        Assert.True(result);
        Assert.True((await context.Employees.FindAsync(emp.Id))!.IsDeleted);
    }
}
