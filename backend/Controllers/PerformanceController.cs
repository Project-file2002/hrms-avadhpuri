using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRMS.API.Models.DTOs.Performance;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager,Manager")]
public class PerformanceController : ControllerBase
{
    private readonly IPerformanceService _performanceService;

    public PerformanceController(IPerformanceService performanceService)
    {
        _performanceService = performanceService;
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> GetAllReviews()
    {
        var reviews = await _performanceService.GetAllAsync();
        return Ok(reviews);
    }

    [HttpGet("reviews/{id}")]
    public async Task<IActionResult> GetReviewById(int id)
    {
        var review = await _performanceService.GetByIdAsync(id);
        if (review == null) return NotFound();
        return Ok(review);
    }

    [HttpPost("reviews")]
    public async Task<IActionResult> CreateReview([FromBody] CreatePerformanceReview request)
    {
        var review = await _performanceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
    }

    [HttpPut("reviews/{id}")]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdatePerformanceReview request)
    {
        var review = await _performanceService.UpdateAsync(id, request);
        if (review == null) return NotFound();
        return Ok(review);
    }

    [HttpDelete("reviews/{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var result = await _performanceService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("cycles")]
    public async Task<IActionResult> GetCycles()
    {
        var cycles = await _performanceService.GetCyclesAsync();
        return Ok(cycles);
    }
}
