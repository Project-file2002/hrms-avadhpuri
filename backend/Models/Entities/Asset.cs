namespace HRMS.API.Models.Entities;

public class Asset
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public string Category { get; set; } = "Other";
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public string Status { get; set; } = "Available";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AssetAllocation> Allocations { get; set; } = new List<AssetAllocation>();
    public ICollection<AssetMaintenance> MaintenanceRecords { get; set; } = new List<AssetMaintenance>();
}

public class AssetAllocation
{
    public int Id { get; set; }
    public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnedAt { get; set; }
    public string? Notes { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}

public class AssetMaintenance
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "Repair";
    public decimal? Cost { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime ScheduledDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;
}
