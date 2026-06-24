namespace HRMS.API.Models.Entities;

public class WorkflowInstance
{
    public int Id { get; set; }
    public int RecordId { get; set; } // The entity record this workflow is running against
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Approved, Rejected
    public string CurrentStep { get; set; } = string.Empty;
    public string? Data { get; set; } // JSON context data
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public int WorkflowDefinitionId { get; set; }
    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
}
