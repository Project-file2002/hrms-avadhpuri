namespace HRMS.API.Models.Entities;

public class Document
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "Other";
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = "Active";

    public int UploadedById { get; set; }
    public Employee UploadedBy { get; set; } = null!;

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}
