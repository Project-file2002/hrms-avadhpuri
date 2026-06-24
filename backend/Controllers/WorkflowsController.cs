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
public class WorkflowsController : ControllerBase
{
    private readonly HRMSDbContext _context;

    public WorkflowsController(HRMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var workflows = await _context.WorkflowDefinitions
            .Include(w => w.Instances)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return Ok(workflows.Select(w => new WorkflowDefinitionDto
        {
            Id = w.Id,
            Name = w.Name,
            Description = w.Description,
            Steps = w.Steps,
            IsActive = w.IsActive,
            CreatedAt = w.CreatedAt,
            InstanceCount = w.Instances.Count
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        if (workflow == null) return NotFound();
        return Ok(workflow);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowDefinitionRequest request)
    {
        var workflow = new WorkflowDefinition
        {
            Name = request.Name,
            Description = request.Description,
            Steps = request.Steps
        };
        _context.WorkflowDefinitions.Add(workflow);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = workflow.Id }, workflow);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateWorkflowDefinitionRequest request)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        if (workflow == null) return NotFound();
        workflow.Name = request.Name;
        workflow.Description = request.Description;
        workflow.Steps = request.Steps;
        workflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(workflow);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        if (workflow == null) return NotFound();
        _context.WorkflowDefinitions.Remove(workflow);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(int id, [FromBody] StartWorkflowRequest request)
    {
        var workflow = await _context.WorkflowDefinitions.FindAsync(id);
        if (workflow == null) return NotFound();

        var steps = System.Text.Json.JsonSerializer.Deserialize<List<WorkflowStep>>(workflow.Steps);
        var firstStep = steps?.FirstOrDefault()?.Name ?? "Step 1";

        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = id,
            RecordId = request.RecordId,
            Status = "InProgress",
            CurrentStep = firstStep,
            Data = request.Data
        };
        _context.WorkflowInstances.Add(instance);
        await _context.SaveChangesAsync();
        return Ok(instance);
    }

    [HttpGet("instances")]
    public async Task<IActionResult> GetInstances()
    {
        var instances = await _context.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Ok(instances.Select(i => new WorkflowInstanceDto
        {
            Id = i.Id,
            RecordId = i.RecordId,
            Status = i.Status,
            CurrentStep = i.CurrentStep,
            Data = i.Data,
            CreatedAt = i.CreatedAt,
            CompletedAt = i.CompletedAt,
            WorkflowName = i.WorkflowDefinition.Name
        }));
    }
}

public class StartWorkflowRequest
{
    public int RecordId { get; set; }
    public string? Data { get; set; }
}

public class WorkflowStep
{
    public string Name { get; set; } = string.Empty;
    public string? AssigneeRole { get; set; }
    public string? Action { get; set; } // Approve, Review, Notify
}
