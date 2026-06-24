namespace HRMS.API.Models.Entities;

public class CustomField
{
    public int Id { get; set; }
    public string Module { get; set; } = string.Empty; // "Employee", "Leave", "Attendance", etc.
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = "Text"; // Text, Number, Date, Select, Boolean
    public string? Options { get; set; } // JSON array for Select type
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
