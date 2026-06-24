using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Auth;
using HRMS.API.Models.Entities;
using HRMS.API.Models.Mapping;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly HRMSDbContext _context;

    public UsersController(HRMSDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();

        var result = users.Select(u => new
        {
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.IsActive,
            u.CreatedAt,
            u.LastLoginAt,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
        });

        return Ok(result);
    }

    [HttpPut("{id}/roles")]
    public async Task<IActionResult> UpdateRoles(int id, [FromBody] UpdateRolesRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        _context.UserRoles.RemoveRange(user.UserRoles);

        var roles = await _context.Roles.Where(r => request.RoleIds.Contains(r.Id)).ToListAsync();
        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { Role = role });
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Roles updated" });
    }
}

public class UpdateRolesRequest
{
    public List<int> RoleIds { get; set; } = new();
}
