namespace HRMS.API.Models.Entities;

public class FormDefinition
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Schema { get; set; } = "[]"; // JSON array of field definitions
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<FormSubmission> Submissions { get; set; } = new List<FormSubmission>();
}
