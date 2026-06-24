using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRMS.API.Models.DTOs.Copilot;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CopilotController : ControllerBase
{
    private readonly ICopilotService _copilotService;

    public CopilotController(ICopilotService copilotService)
    {
        _copilotService = copilotService;
    }

    [HttpGet("welcome")]
    public async Task<IActionResult> Welcome()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var response = await _copilotService.GetWelcomeAsync(userId);
        return Ok(response);
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] CopilotChatRequest request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var response = await _copilotService.ProcessMessageAsync(request.Message, userId);
        return Ok(response);
    }
}
