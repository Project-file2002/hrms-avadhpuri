using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.NoCode;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class CustomFieldsController : ControllerBase
{
    private readonly HRMSDbContext _context;

    public CustomFieldsController(HRMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? module)
    {
        var query = _context.CustomFields.AsQueryable();
        if (!string.IsNullOrEmpty(module))
            query = query.Where(cf => cf.Module == module);

        var fields = await query.OrderBy(cf => cf.Module).ThenBy(cf => cf.SortOrder).ToListAsync();
        return Ok(fields.Select(f => f.ToDto()));
    }

    [HttpGet("modules")]
    public IActionResult GetModules()
    {
        var modules = new[] { "Employee", "Leave", "Attendance", "Performance", "Payroll", "Recruitment" };
        return Ok(modules);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomFieldRequest request)
    {
        var field = new CustomField
        {
            Module = request.Module,
            FieldName = request.FieldName,
            FieldType = request.FieldType,
            Options = request.Options,
            IsRequired = request.IsRequired,
            SortOrder = request.SortOrder
        };
        _context.CustomFields.Add(field);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { module = field.Module }, field.ToDto());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCustomFieldRequest request)
    {
        var field = await _context.CustomFields.FindAsync(id);
        if (field == null) return NotFound();
        field.FieldName = request.FieldName;
        field.FieldType = request.FieldType;
        field.Options = request.Options;
        field.IsRequired = request.IsRequired;
        field.SortOrder = request.SortOrder;
        await _context.SaveChangesAsync();
        return Ok(field.ToDto());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var field = await _context.CustomFields.FindAsync(id);
        if (field == null) return NotFound();
        _context.CustomFields.Remove(field);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("values/{module}/{recordId}")]
    public async Task<IActionResult> GetValues(string module, int recordId)
    {
        var fieldIds = await _context.CustomFields
            .Where(cf => cf.Module == module && cf.IsActive)
            .Select(cf => cf.Id)
            .ToListAsync();

        var values = await _context.CustomFieldValues
            .Where(v => fieldIds.Contains(v.CustomFieldId) && v.RecordId == recordId)
            .ToListAsync();

        return Ok(values.Select(v => new { v.CustomFieldId, v.Value }));
    }

    [HttpPost("values/{module}/{recordId}")]
    public async Task<IActionResult> SaveValues(string module, int recordId, [FromBody] List<CustomFieldValueDto> values)
    {
        var fieldIds = await _context.CustomFields
            .Where(cf => cf.Module == module && cf.IsActive)
            .Select(cf => cf.Id)
            .ToListAsync();

        var existing = await _context.CustomFieldValues
            .Where(v => fieldIds.Contains(v.CustomFieldId) && v.RecordId == recordId)
            .ToListAsync();

        _context.CustomFieldValues.RemoveRange(existing);

        foreach (var v in values.Where(v => fieldIds.Contains(v.CustomFieldId)))
        {
            _context.CustomFieldValues.Add(new CustomFieldValue
            {
                CustomFieldId = v.CustomFieldId,
                RecordId = recordId,
                Value = v.Value ?? string.Empty
            });
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}

public class CustomFieldValueDto
{
    public int CustomFieldId { get; set; }
    public string? Value { get; set; }
}

public static class CustomFieldMappings
{
    public static CustomFieldDto ToDto(this CustomField cf) => new()
    {
        Id = cf.Id,
        Module = cf.Module,
        FieldName = cf.FieldName,
        FieldType = cf.FieldType,
        Options = cf.Options,
        IsRequired = cf.IsRequired,
        SortOrder = cf.SortOrder,
        IsActive = cf.IsActive
    };
}
