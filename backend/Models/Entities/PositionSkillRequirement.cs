namespace HRMS.API.Models.Entities;

public class PositionSkillRequirement
{
    public int Id { get; set; }
    public string Position { get; set; } = string.Empty;
    public int MinimumProficiency { get; set; } = 3;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
}
