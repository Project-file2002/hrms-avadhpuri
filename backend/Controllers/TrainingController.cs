using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager,Manager")]
public class TrainingController : ControllerBase
{
    private readonly HRMSDbContext _context;
    public TrainingController(HRMSDbContext context) => _context = context;

    // === COURSES ===

    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses()
    {
        var courses = await _context.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Enrollments).ThenInclude(e => e.Employee)
            .OrderByDescending(c => c.CreatedAt).ToListAsync();
        return Ok(courses.Select(c => new
        {
            c.Id, c.Title, c.Description, c.Category, c.Instructor, c.DurationHours, c.MaxCapacity, c.Status, c.CreatedAt,
            CreatedByName = c.CreatedBy != null ? $"{c.CreatedBy.FirstName} {c.CreatedBy.LastName}" : null,
            EnrolledCount = c.Enrollments.Count,
            CompletedCount = c.Enrollments.Count(e => e.Status == "Completed"),
            Enrollments = c.Enrollments.Select(e => new
            {
                e.Id, e.Status, e.Score, e.EnrolledAt, e.CompletedAt,
                EmployeeName = $"{e.Employee.FirstName} {e.Employee.LastName}",
                e.EmployeeId
            })
        }));
    }

    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _context.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == userId);
        var course = new Course
        {
            Title = request.Title, Description = request.Description, Category = request.Category,
            Instructor = request.Instructor, DurationHours = request.DurationHours,
            MaxCapacity = request.MaxCapacity, CreatedById = user?.EmployeeId
        };
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return Ok(new { course.Id });
    }

    [HttpPost("courses/{id}/enroll")]
    public async Task<IActionResult> Enroll(int id, [FromBody] EnrollRequest request)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound();
        var count = await _context.TrainingEnrollments.CountAsync(e => e.CourseId == id);
        if (count >= course.MaxCapacity) return BadRequest("Course is full");

        var enrollment = new TrainingEnrollment { CourseId = id, EmployeeId = request.EmployeeId };
        _context.TrainingEnrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return Ok(new { enrollment.Id });
    }

    [HttpPut("enrollments/{id}")]
    public async Task<IActionResult> UpdateEnrollment(int id, [FromBody] UpdateEnrollmentRequest request)
    {
        var enrollment = await _context.TrainingEnrollments.FindAsync(id);
        if (enrollment == null) return NotFound();
        enrollment.Status = request.Status;
        enrollment.Score = request.Score;
        enrollment.CompletedAt = request.Status == "Completed" ? DateTime.UtcNow : null;
        await _context.SaveChangesAsync();
        return Ok(enrollment);
    }

    // === CERTIFICATIONS ===

    [HttpGet("certifications")]
    public async Task<IActionResult> GetCertifications()
    {
        var certs = await _context.Certifications.ToListAsync();
        return Ok(certs);
    }

    [HttpPost("certifications")]
    public async Task<IActionResult> CreateCertification([FromBody] CreateCertificationRequest request)
    {
        var cert = new Certification
        {
            Name = request.Name, Description = request.Description,
            Issuer = request.Issuer, ExpiryDays = request.ExpiryDays
        };
        _context.Certifications.Add(cert);
        await _context.SaveChangesAsync();
        return Ok(cert);
    }

    [HttpPost("employee-certifications")]
    public async Task<IActionResult> AddEmployeeCert([FromBody] AddEmployeeCertRequest request)
    {
        var ec = new EmployeeCertification
        {
            EmployeeId = request.EmployeeId, CertificationId = request.CertificationId,
            ObtainedAt = request.ObtainedAt,
            ExpiryDate = request.ExpiryDate, CredentialUrl = request.CredentialUrl
        };
        _context.EmployeeCertifications.Add(ec);
        await _context.SaveChangesAsync();
        return Ok(ec);
    }

    [HttpGet("employee-certifications/{employeeId}")]
    public async Task<IActionResult> GetEmployeeCerts(int employeeId)
    {
        var certs = await _context.EmployeeCertifications
            .Include(ec => ec.Certification)
            .Where(ec => ec.EmployeeId == employeeId).ToListAsync();
        return Ok(certs.Select(ec => new
        {
            ec.Id, ec.ObtainedAt, ec.ExpiryDate, ec.Status, ec.CredentialUrl,
            Certification = new { ec.Certification.Id, ec.Certification.Name, ec.Certification.Issuer }
        }));
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var courses = await _context.Courses.CountAsync();
        var active = await _context.Courses.CountAsync(c => c.Status == "Active");
        var enrollments = await _context.TrainingEnrollments.CountAsync();
        var completed = await _context.TrainingEnrollments.CountAsync(e => e.Status == "Completed");
        var certs = await _context.Certifications.CountAsync();
        var empCerts = await _context.EmployeeCertifications.CountAsync();
        return Ok(new { TotalCourses = courses, ActiveCourses = active, TotalEnrollments = enrollments, CompletedEnrollments = completed, TotalCertifications = certs, EmployeeCertifications = empCerts });
    }
}

public record CreateCourseRequest(string Title, string? Description, string Category, string? Instructor, double DurationHours, int MaxCapacity);
public record EnrollRequest(int EmployeeId);
public record UpdateEnrollmentRequest(string Status, double? Score);
public record CreateCertificationRequest(string Name, string? Description, string? Issuer, int ExpiryDays);
public record AddEmployeeCertRequest(int EmployeeId, int CertificationId, DateTime ObtainedAt, DateTime? ExpiryDate, string? CredentialUrl);
