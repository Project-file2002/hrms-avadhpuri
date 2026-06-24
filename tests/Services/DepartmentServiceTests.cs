using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Department;
using HRMS.API.Models.Entities;
using HRMS.API.Services;

namespace HRMS.API.Tests.Services;

public class DepartmentServiceTests
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
        context.Departments.AddRange(
            new Department { Name = "Engineering" },
            new Department { Name = "HR", IsDeleted = true }
        );
        await context.SaveChangesAsync();

        var service = new DepartmentService(context);
        var result = await service.GetAllAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task CreateAsync_AddsDepartment()
    {
        var context = CreateContext();
        var service = new DepartmentService(context);

        var result = await service.CreateAsync(new CreateDepartmentRequest
        {
            Name = "Marketing",
            Description = "Marketing department"
        });

        Assert.Equal("Marketing", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletes()
    {
        var context = CreateContext();
        var dept = new Department { Name = "Finance" };
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        var service = new DepartmentService(context);
        var result = await service.DeleteAsync(dept.Id);

        Assert.True(result);
        Assert.True((await context.Departments.FindAsync(dept.Id))!.IsDeleted);
    }
}
