using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Performance;
using HRMS.API.Models.Mapping;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Services;

public class PerformanceService : IPerformanceService
{
    private readonly HRMSDbContext _context;

    public PerformanceService(HRMSDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PerformanceReviewDto>> GetAllAsync()
    {
        var reviews = await _context.PerformanceReviews
            .Include(pr => pr.Employee)
            .Include(pr => pr.Reviewer)
            .Include(pr => pr.Cycle)
            .Include(pr => pr.Scores)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();

        return reviews.Select(pr => pr.ToDto());
    }

    public async Task<PerformanceReviewDto?> GetByIdAsync(int id)
    {
        var review = await _context.PerformanceReviews
            .Include(pr => pr.Employee)
            .Include(pr => pr.Reviewer)
            .Include(pr => pr.Cycle)
            .Include(pr => pr.Scores)
            .FirstOrDefaultAsync(pr => pr.Id == id);

        return review?.ToDto();
    }

    public async Task<PerformanceReviewDto> CreateAsync(CreatePerformanceReview request)
    {
        var review = new Models.Entities.PerformanceReview
        {
            Title = request.Title,
            EmployeeId = request.EmployeeId,
            CycleId = request.CycleId,
            ReviewerId = request.EmployeeId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1)
        };

        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        await _context.Entry(review).Reference(pr => pr.Employee).LoadAsync();
        await _context.Entry(review).Reference(pr => pr.Reviewer).LoadAsync();
        await _context.Entry(review).Reference(pr => pr.Cycle).LoadAsync();

        return review.ToDto();
    }

    public async Task<PerformanceReviewDto?> UpdateAsync(int id, UpdatePerformanceReview request)
    {
        var review = await _context.PerformanceReviews
            .Include(pr => pr.Employee)
            .Include(pr => pr.Reviewer)
            .Include(pr => pr.Cycle)
            .Include(pr => pr.Scores)
            .FirstOrDefaultAsync(pr => pr.Id == id);
        if (review == null) return null;

        if (request.Title != null) review.Title = request.Title;
        if (request.Comments != null) review.Comments = request.Comments;
        if (request.Status != null) review.Status = request.Status;
        if (request.CycleId.HasValue) review.CycleId = request.CycleId.Value;

        if (request.Scores != null)
        {
            _context.ReviewScores.RemoveRange(review.Scores);
            review.Scores = request.Scores.Select(s => new Models.Entities.ReviewScore
            {
                Criteria = s.Criteria,
                Score = s.Score,
                Comments = s.Comments
            }).ToList();
            review.OverallScore = Math.Round(review.Scores.Average(s => s.Score), 1);
        }

        await _context.SaveChangesAsync();
        return review.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var review = await _context.PerformanceReviews.FindAsync(id);
        if (review == null) return false;
        _context.PerformanceReviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ReviewCycleDto>> GetCyclesAsync()
    {
        var cycles = await _context.ReviewCycles.ToListAsync();
        return cycles.Select(rc => rc.ToDto());
    }
}
