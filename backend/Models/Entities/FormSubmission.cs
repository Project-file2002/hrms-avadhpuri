namespace HRMS.API.Models.Entities;

public class FormSubmission
{
    public int Id { get; set; }
    public string Data { get; set; } = "{}"; // JSON of submitted values
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public int FormDefinitionId { get; set; }
    public FormDefinition FormDefinition { get; set; } = null!;
}
