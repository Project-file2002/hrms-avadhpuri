namespace HRMS.API.Models.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Technical";
    public string? Description { get; set; }
}

public class EmployeeSkill
{
    public int Id { get; set; }
    public int ProficiencyLevel { get; set; } = 1;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
}

public class TalentPool
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CreatedById { get; set; }
    public Employee CreatedBy { get; set; } = null!;

    public ICollection<TalentPoolCandidate> Candidates { get; set; } = new List<TalentPoolCandidate>();
}

public class TalentPoolCandidate
{
    public int Id { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public int TalentPoolId { get; set; }
    public TalentPool Pool { get; set; } = null!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}
