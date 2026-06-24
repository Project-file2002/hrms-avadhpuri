using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager,Manager")]
public class SkillsController : ControllerBase
{
    private readonly HRMSDbContext _context;
    public SkillsController(HRMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetSkills()
    {
        var skills = await _context.Skills.OrderBy(s => s.Category).ThenBy(s => s.Name).ToListAsync();
        return Ok(skills);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillRequest request)
    {
        var skill = new Skill { Name = request.Name, Category = request.Category, Description = request.Description };
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();
        return Ok(skill);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill == null) return NotFound();
        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeSkills(int employeeId)
    {
        var skills = await _context.EmployeeSkills
            .Include(es => es.Skill)
            .Where(es => es.EmployeeId == employeeId)
            .ToListAsync();
        return Ok(skills.Select(es => new { es.Id, es.ProficiencyLevel, Skill = new { es.Skill.Id, es.Skill.Name, es.Skill.Category } }));
    }

    [HttpPost("employee")]
    public async Task<IActionResult> AddEmployeeSkill([FromBody] AddEmployeeSkillRequest request)
    {
        var es = new EmployeeSkill
        { EmployeeId = request.EmployeeId, SkillId = request.SkillId, ProficiencyLevel = request.ProficiencyLevel };
        _context.EmployeeSkills.Add(es);
        await _context.SaveChangesAsync();
        return Ok(es);
    }

    [HttpPut("employee/{id}")]
    public async Task<IActionResult> UpdateEmployeeSkill(int id, [FromBody] UpdateEmployeeSkillRequest request)
    {
        var es = await _context.EmployeeSkills.FindAsync(id);
        if (es == null) return NotFound();
        es.ProficiencyLevel = request.ProficiencyLevel;
        await _context.SaveChangesAsync();
        return Ok(es);
    }

    [HttpDelete("employee/{id}")]
    public async Task<IActionResult> RemoveEmployeeSkill(int id)
    {
        var es = await _context.EmployeeSkills.FindAsync(id);
        if (es == null) return NotFound();
        _context.EmployeeSkills.Remove(es);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var totalSkills = await _context.Skills.CountAsync();
        var employees = await _context.Employees.Where(e => !e.IsDeleted).CountAsync();
        var empSkills = await _context.EmployeeSkills.GroupBy(es => es.EmployeeId).CountAsync();
        var skillGaps = await _context.Skills
            .GroupJoin(_context.EmployeeSkills, s => s.Id, es => es.SkillId, (s, es) => new { s.Name, s.Category, Count = es.Count() })
            .OrderBy(x => x.Count).Take(5).ToListAsync();

        var categories = await _context.Skills
            .GroupBy(s => s.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        var coverage = employees > 0 ? $"{Math.Round((double)empSkills / employees * 100, 1)}%" : "0%";
        return Ok(new
        {
            TotalSkills = totalSkills, EmployeesWithSkills = empSkills, TotalEmployees = employees,
            SkillCoverage = coverage,
            ScarceSkills = skillGaps,
            Categories = categories
        });
    }

    // === TALENT POOL ===

    [HttpGet("pools")]
    public async Task<IActionResult> GetPools()
    {
        var pools = await _context.TalentPools
            .Include(p => p.CreatedBy)
            .Include(p => p.Candidates).ThenInclude(c => c.Employee)
            .OrderByDescending(p => p.CreatedAt).ToListAsync();
        return Ok(pools.Select(p => new
        {
            p.Id, p.Name, p.Description, p.CreatedAt,
            CreatedByName = $"{p.CreatedBy.FirstName} {p.CreatedBy.LastName}",
            CandidateCount = p.Candidates.Count,
            Candidates = p.Candidates.Select(c => new
            {
                c.Id, c.Status, c.Notes, c.AddedAt,
                EmployeeName = $"{c.Employee.FirstName} {c.Employee.LastName}",
                c.Employee.Position, c.EmployeeId
            })
        }));
    }

    [HttpPost("pools")]
    public async Task<IActionResult> CreatePool([FromBody] CreatePoolRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();
        var pool = new TalentPool { Name = request.Name, Description = request.Description, CreatedById = user.EmployeeId.Value };
        _context.TalentPools.Add(pool);
        await _context.SaveChangesAsync();
        return Ok(pool);
    }

    [HttpPost("pools/{poolId}/candidates")]
    public async Task<IActionResult> AddCandidate(int poolId, [FromBody] AddPoolCandidateRequest request)
    {
        var candidate = new TalentPoolCandidate
        { TalentPoolId = poolId, EmployeeId = request.EmployeeId, Status = "Active", Notes = request.Notes };
        _context.TalentPoolCandidates.Add(candidate);
        await _context.SaveChangesAsync();
        return Ok(candidate);
    }

    [HttpDelete("pools/candidates/{id}")]
    public async Task<IActionResult> RemoveCandidate(int id)
    {
        var c = await _context.TalentPoolCandidates.FindAsync(id);
        if (c == null) return NotFound();
        _context.TalentPoolCandidates.Remove(c);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // === POSITION SKILL REQUIREMENTS ===

    [HttpGet("position-requirements")]
    public async Task<IActionResult> GetPositionRequirements()
    {
        var reqs = await _context.PositionSkillRequirements
            .Include(r => r.Skill)
            .OrderBy(r => r.Position).ThenBy(r => r.Skill.Name)
            .ToListAsync();
        return Ok(reqs.Select(r => new
        {
            r.Id, r.Position, r.MinimumProficiency,
            Skill = new { r.Skill.Id, r.Skill.Name, r.Skill.Category }
        }));
    }

    [HttpPost("position-requirements")]
    public async Task<IActionResult> AddPositionRequirement([FromBody] AddPositionRequirementRequest request)
    {
        var existing = await _context.PositionSkillRequirements
            .AnyAsync(r => r.Position == request.Position && r.SkillId == request.SkillId);
        if (existing) return Conflict("Requirement already exists for this position and skill.");
        var req = new PositionSkillRequirement
        { Position = request.Position, SkillId = request.SkillId, MinimumProficiency = request.MinimumProficiency };
        _context.PositionSkillRequirements.Add(req);
        await _context.SaveChangesAsync();
        return Ok(req);
    }

    [HttpDelete("position-requirements/{id}")]
    public async Task<IActionResult> DeletePositionRequirement(int id)
    {
        var req = await _context.PositionSkillRequirements.FindAsync(id);
        if (req == null) return NotFound();
        _context.PositionSkillRequirements.Remove(req);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // === SKILL GAP ANALYSIS ===

    [HttpGet("employee-gap/{employeeId}")]
    public async Task<IActionResult> GetEmployeeSkillGap(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null || string.IsNullOrEmpty(employee.Position))
            return Ok(new { Position = "", HasPosition = false, Requirements = Array.Empty<object>(), EmployeeSkills = Array.Empty<object>(), Gaps = Array.Empty<object>(), CoveragePercent = 0 });

        var reqs = await _context.PositionSkillRequirements
            .Include(r => r.Skill)
            .Where(r => r.Position == employee.Position)
            .ToListAsync();

        var empSkills = await _context.EmployeeSkills
            .Include(es => es.Skill)
            .Where(es => es.EmployeeId == employeeId)
            .ToListAsync();

        var gaps = reqs.Select(r =>
        {
            var empSkill = empSkills.FirstOrDefault(es => es.SkillId == r.SkillId);
            var current = empSkill?.ProficiencyLevel ?? 0;
            return new
            {
                SkillId = r.SkillId,
                SkillName = r.Skill.Name,
                SkillCategory = r.Skill.Category,
                RequiredProficiency = r.MinimumProficiency,
                CurrentProficiency = current,
                Gap = Math.Max(0, r.MinimumProficiency - current),
                Met = current >= r.MinimumProficiency
            };
        }).ToList();

        var total = gaps.Count;
        var met = gaps.Count(g => g.Met);
        return Ok(new
        {
            employee.Position,
            HasPosition = true,
            Requirements = reqs.Select(r => new { r.SkillId, SkillName = r.Skill.Name, r.MinimumProficiency }),
            EmployeeSkills = empSkills.Select(es => new { es.SkillId, SkillName = es.Skill.Name, es.ProficiencyLevel }),
            Gaps = gaps,
            CoveragePercent = total > 0 ? Math.Round((double)met / total * 100, 1) : 0
        });
    }

    [HttpGet("employee-gap")]
    public async Task<IActionResult> GetAllEmployeeGaps()
    {
        var employees = await _context.Employees.Where(e => !e.IsDeleted && e.Position != null).ToListAsync();
        var reqs = await _context.PositionSkillRequirements.Include(r => r.Skill).ToListAsync();
        var empSkills = await _context.EmployeeSkills.ToListAsync();

        var results = employees.Select(e =>
        {
            var posReqs = reqs.Where(r => r.Position == e.Position).ToList();
            if (!posReqs.Any()) return null;
            var matched = posReqs.Count(r => empSkills.Any(es => es.EmployeeId == e.Id && es.SkillId == r.SkillId && es.ProficiencyLevel >= r.MinimumProficiency));
            return new
            {
                EmployeeId = e.Id,
                EmployeeName = $"{e.FirstName} {e.LastName}",
                e.Position,
                RequiredSkills = posReqs.Count,
                MetSkills = matched,
                CoveragePercent = Math.Round((double)matched / posReqs.Count * 100, 1)
            };
        }).Where(r => r != null).OrderBy(r => r!.CoveragePercent).ToList();

        return Ok(results);
    }
}

public record AddPositionRequirementRequest(string Position, int SkillId, int MinimumProficiency = 3);
public record CreateSkillRequest(string Name, string Category, string? Description);
public record AddEmployeeSkillRequest(int EmployeeId, int SkillId, int ProficiencyLevel);
public record UpdateEmployeeSkillRequest(int ProficiencyLevel);
public record CreatePoolRequest(string Name, string? Description);
public record AddPoolCandidateRequest(int EmployeeId, string? Notes);
