using HRMS.API.Models.DTOs.Predictive;

namespace HRMS.API.Services.Interfaces;

public interface IPredictiveService
{
    Task<PredictiveDashboardDto> GetDashboardAsync();
    Task<List<AttritionRiskDto>> GetAttritionRisksAsync();
    Task<HiringForecastDto> GetHiringForecastAsync();
    Task<List<BurnoutRiskDto>> GetBurnoutRisksAsync();
}
