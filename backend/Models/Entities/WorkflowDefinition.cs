namespace HRMS.API.Models.Entities;

public class WorkflowDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Steps { get; set; } = "[]"; // JSON array of workflow steps
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
}
