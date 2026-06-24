namespace HRMS.API.Models.DTOs.Predictive;

public class AttritionRiskDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public double RiskScore { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public List<string> RiskFactors { get; set; } = new();
    public string? SuggestedAction { get; set; }
}

public class HiringForecastDto
{
    public int OpenPositions { get; set; }
    public int TotalCandidates { get; set; }
    public double AvgDaysOpen { get; set; }
    public List<PositionForecastDto> Positions { get; set; } = new();
}

public class PositionForecastDto
{
    public string Title { get; set; } = string.Empty;
    public string? Department { get; set; }
    public int DaysOpen { get; set; }
    public int CandidateCount { get; set; }
    public string Urgency { get; set; } = "Normal";
}

public class BurnoutRiskDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public double BurnoutScore { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public List<string> Indicators { get; set; } = new();
    public string? SuggestedAction { get; set; }
}

public class PredictiveDashboardDto
{
    public List<AttritionRiskDto> AttritionRisks { get; set; } = new();
    public HiringForecastDto HiringForecast { get; set; } = new();
    public List<BurnoutRiskDto> BurnoutRisks { get; set; } = new();
    public DashboardSummaryDto Summary { get; set; } = new();
}

public class DashboardSummaryDto
{
    public int TotalEmployees { get; set; }
    public int AtRiskEmployees { get; set; }
    public int HighBurnoutEmployees { get; set; }
    public int OpenPositions { get; set; }
    public double AvgRiskScore { get; set; }
    public double AvgBurnoutScore { get; set; }
}
