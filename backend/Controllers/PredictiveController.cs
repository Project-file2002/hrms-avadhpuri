using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRMS.API.Models.DTOs.Predictive;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager,Manager")]
public class PredictiveController : ControllerBase
{
    private readonly IPredictiveService _predictiveService;

    public PredictiveController(IPredictiveService predictiveService)
    {
        _predictiveService = predictiveService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _predictiveService.GetDashboardAsync();
        return Ok(dashboard);
    }

    [HttpGet("attrition-risk")]
    public async Task<IActionResult> GetAttritionRisks()
    {
        var risks = await _predictiveService.GetAttritionRisksAsync();
        return Ok(risks);
    }

    [HttpGet("hiring-forecast")]
    public async Task<IActionResult> GetHiringForecast()
    {
        var forecast = await _predictiveService.GetHiringForecastAsync();
        return Ok(forecast);
    }

    [HttpGet("burnout-detection")]
    public async Task<IActionResult> GetBurnoutRisks()
    {
        var risks = await _predictiveService.GetBurnoutRisksAsync();
        return Ok(risks);
    }
}
