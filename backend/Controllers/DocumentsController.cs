using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly HRMSDbContext _context;
    public DocumentsController(HRMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? employeeId)
    {
        var query = _context.Documents
            .Include(d => d.UploadedBy)
            .Include(d => d.Employee)
            .AsQueryable();
        if (employeeId.HasValue)
            query = query.Where(d => d.EmployeeId == employeeId);
        var docs = await query.OrderByDescending(d => d.UploadedAt).ToListAsync();
        return Ok(docs.Select(MapDoc));
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromBody] UploadDocumentRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.EmployeeId == null) return Unauthorized();

        var doc = new Document
        {
            Title = request.Title, Description = request.Description, Category = request.Category,
            FileName = request.FileName, FileSize = request.FileSize, FileType = request.FileType,
            FilePath = request.FilePath, EmployeeId = request.EmployeeId,
            ExpiryDate = request.ExpiryDate, UploadedById = user.EmployeeId.Value
        };
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();

        // Auto-create notification for employee
        if (request.EmployeeId.HasValue)
        {
            var empUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == request.EmployeeId);
            if (empUser != null)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = empUser.Id, Title = "New Document Uploaded",
                    Message = $"Document '{request.Title}' has been uploaded to your profile.",
                    Type = "Document", Link = "/documents"
                });
                await _context.SaveChangesAsync();
            }
        }
        return Ok(new { doc.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _context.Documents.FindAsync(id);
        if (doc == null) return NotFound();
        _context.Documents.Remove(doc);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static object MapDoc(Document d) => new
    {
        d.Id, d.Title, d.Description, d.Category, d.FileName, d.FileSize, d.FileType, d.FilePath,
        d.UploadedAt, d.ExpiryDate, d.Status,
        UploadedByName = $"{d.UploadedBy.FirstName} {d.UploadedBy.LastName}",
        EmployeeName = d.Employee != null ? $"{d.Employee.FirstName} {d.Employee.LastName}" : null,
        d.EmployeeId
    };
}

public record UploadDocumentRequest(string Title, string? Description, string Category, string FileName, long FileSize, string FileType, string? FilePath, int? EmployeeId, DateTime? ExpiryDate);
