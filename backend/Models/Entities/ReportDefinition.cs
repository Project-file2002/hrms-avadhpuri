namespace HRMS.API.Models.Entities;

public class ReportDefinition
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty; // "Employee", "Leave", "Attendance", "Performance"
    public string Columns { get; set; } = "[]"; // JSON array of selected column names
    public string? Filters { get; set; } // JSON filter conditions
    public string? GroupBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
