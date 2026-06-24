using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.NoCode;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FormsController : ControllerBase
{
    private readonly HRMSDbContext _context;

    public FormsController(HRMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var forms = await _context.FormDefinitions
            .Include(f => f.Submissions)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return Ok(forms.Select(f => new FormDefinitionDto
        {
            Id = f.Id,
            Title = f.Title,
            Description = f.Description,
            Schema = f.Schema,
            IsActive = f.IsActive,
            CreatedAt = f.CreatedAt,
            SubmissionCount = f.Submissions.Count
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var form = await _context.FormDefinitions.FindAsync(id);
        if (form == null) return NotFound();
        return Ok(form);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFormDefinitionRequest request)
    {
        var form = new FormDefinition
        {
            Title = request.Title,
            Description = request.Description,
            Schema = request.Schema
        };
        _context.FormDefinitions.Add(form);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = form.Id }, form);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateFormDefinitionRequest request)
    {
        var form = await _context.FormDefinitions.FindAsync(id);
        if (form == null) return NotFound();
        form.Title = request.Title;
        form.Description = request.Description;
        form.Schema = request.Schema;
        form.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(form);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var form = await _context.FormDefinitions.FindAsync(id);
        if (form == null) return NotFound();
        _context.FormDefinitions.Remove(form);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(int id, [FromBody] SubmitFormRequest request)
    {
        var form = await _context.FormDefinitions.FindAsync(id);
        if (form == null) return NotFound();

        var userName = User.Identity?.Name ?? "Anonymous";
        var submission = new FormSubmission
        {
            FormDefinitionId = id,
            Data = request.Data,
            SubmittedBy = userName,
            SubmittedAt = DateTime.UtcNow
        };
        _context.FormSubmissions.Add(submission);
        await _context.SaveChangesAsync();
        return Ok(submission);
    }

    [HttpGet("{id}/submissions")]
    public async Task<IActionResult> GetSubmissions(int id)
    {
        var submissions = await _context.FormSubmissions
            .Where(s => s.FormDefinitionId == id)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

        return Ok(submissions.Select(s => new FormSubmissionDto
        {
            Id = s.Id,
            Data = s.Data,
            SubmittedBy = s.SubmittedBy,
            SubmittedAt = s.SubmittedAt,
            FormDefinitionId = s.FormDefinitionId
        }));
    }
}
