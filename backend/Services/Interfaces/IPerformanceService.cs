using HRMS.API.Models.DTOs.Performance;

namespace HRMS.API.Services.Interfaces;

public interface IPerformanceService
{
    Task<IEnumerable<PerformanceReviewDto>> GetAllAsync();
    Task<PerformanceReviewDto?> GetByIdAsync(int id);
    Task<PerformanceReviewDto> CreateAsync(CreatePerformanceReview request);
    Task<PerformanceReviewDto?> UpdateAsync(int id, UpdatePerformanceReview request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<ReviewCycleDto>> GetCyclesAsync();
}
