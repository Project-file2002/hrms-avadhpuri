namespace HRMS.API.Models.Entities;

public class CustomFieldValue
{
    public int Id { get; set; }
    public int RecordId { get; set; } // ID of the entity record
    public string Value { get; set; } = string.Empty;

    public int CustomFieldId { get; set; }
    public CustomField CustomField { get; set; } = null!;
}
