namespace HRMS.API.Models.DTOs.Copilot;

public class CopilotChatRequest
{
    public string Message { get; set; } = string.Empty;
}

public class CopilotResponse
{
    public string Reply { get; set; } = string.Empty;
    public string Intent { get; set; } = "general";
    public string Persona { get; set; } = "employee";
    public bool Restricted { get; set; }
    public List<CopilotAction> Actions { get; set; } = new();
    public object? Data { get; set; }
}

public class CopilotWelcomeResponse
{
    public string Persona { get; set; } = "employee";
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Welcome { get; set; } = string.Empty;
    public List<string> SuggestedPrompts { get; set; } = new();
}

public class CopilotAction
{
    public string Label { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "navigate:/path" or "open:dialog"
}
