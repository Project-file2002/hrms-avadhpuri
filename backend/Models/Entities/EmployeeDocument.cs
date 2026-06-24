namespace HRMS.API.Models.Entities;

public class EmployeeDocument
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}
