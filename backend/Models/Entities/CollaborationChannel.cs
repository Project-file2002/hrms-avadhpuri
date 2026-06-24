namespace HRMS.API.Models.Entities;

public class CollaborationChannel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ChannelType { get; set; } = "Department";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<ChannelMessage> Messages { get; set; } = new List<ChannelMessage>();
}

public class ChannelMessage
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Message";
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ChannelId { get; set; }
    public CollaborationChannel Channel { get; set; } = null!;

    public int AuthorId { get; set; }
    public Employee Author { get; set; } = null!;
}
