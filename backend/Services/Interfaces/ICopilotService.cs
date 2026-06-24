using HRMS.API.Models.DTOs.Copilot;

namespace HRMS.API.Services.Interfaces;

public interface ICopilotService
{
    Task<CopilotResponse> ProcessMessageAsync(string message, int userId);
    Task<CopilotWelcomeResponse> GetWelcomeAsync(int userId);
}
