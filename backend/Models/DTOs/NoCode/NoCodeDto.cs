namespace HRMS.API.Models.DTOs.NoCode;

// --- Form Builder ---
public class FormDefinitionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Schema { get; set; } = "[]";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SubmissionCount { get; set; }
}

public class CreateFormDefinitionRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Schema { get; set; } = "[]";
}

public class FormSubmissionDto
{
    public int Id { get; set; }
    public string Data { get; set; } = "{}";
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public int FormDefinitionId { get; set; }
}

public class SubmitFormRequest
{
    public string Data { get; set; } = "{}";
}

// --- Custom Fields ---
public class CustomFieldDto
{
    public int Id { get; set; }
    public string Module { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = "Text";
    public string? Options { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCustomFieldRequest
{
    public string Module { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = "Text";
    public string? Options { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}

// --- Workflow ---
public class WorkflowDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Steps { get; set; } = "[]";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int InstanceCount { get; set; }
}

public class CreateWorkflowDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Steps { get; set; } = "[]";
}

public class WorkflowInstanceDto
{
    public int Id { get; set; }
    public int RecordId { get; set; }
    public string Status { get; set; } = "Pending";
    public string CurrentStep { get; set; } = string.Empty;
    public string? Data { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
}

// --- Report ---
public class ReportDefinitionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string Columns { get; set; } = "[]";
    public string? Filters { get; set; }
    public string? GroupBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReportDefinitionRequest
{
    public string Title { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string Columns { get; set; } = "[]";
    public string? Filters { get; set; }
    public string? GroupBy { get; set; }
}

public class ReportResultDto
{
    public string Title { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public List<string> ColumnNames { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
}
