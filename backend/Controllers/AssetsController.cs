using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.API.Data;
using HRMS.API.Models.Entities;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,HRManager")]
public class AssetsController : ControllerBase
{
    private readonly HRMSDbContext _context;
    public AssetsController(HRMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var assets = await _context.Assets
            .Include(a => a.Allocations).ThenInclude(aa => aa.Employee)
            .Include(a => a.MaintenanceRecords)
            .OrderByDescending(a => a.CreatedAt).ToListAsync();
        return Ok(assets.Select(MapAsset));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssetRequest request)
    {
        var asset = new Asset
        {
            Name = request.Name, AssetTag = request.AssetTag, Category = request.Category,
            Model = request.Model, SerialNumber = request.SerialNumber,
            PurchaseDate = request.PurchaseDate, PurchasePrice = request.PurchasePrice,
            WarrantyExpiry = request.WarrantyExpiry, Notes = request.Notes
        };
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return Ok(new { asset.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateAssetRequest request)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();
        asset.Name = request.Name; asset.AssetTag = request.AssetTag; asset.Category = request.Category;
        asset.Model = request.Model; asset.SerialNumber = request.SerialNumber;
        asset.PurchaseDate = request.PurchaseDate; asset.PurchasePrice = request.PurchasePrice;
        asset.WarrantyExpiry = request.WarrantyExpiry; asset.Notes = request.Notes;
        await _context.SaveChangesAsync();
        return Ok(asset);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusReq request)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();
        asset.Status = request.Status;
        await _context.SaveChangesAsync();
        return Ok(asset);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();
        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // === ALLOCATIONS ===

    [HttpPost("{id}/allocate")]
    public async Task<IActionResult> Allocate(int id, [FromBody] AllocateRequest request)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();
        if (asset.Status != "Available") return BadRequest("Asset is not available");

        var allocation = new AssetAllocation { AssetId = id, EmployeeId = request.EmployeeId, Notes = request.Notes };
        _context.AssetAllocations.Add(allocation);
        asset.Status = "Allocated";
        await _context.SaveChangesAsync();
        return Ok(new { allocation.Id });
    }

    [HttpPost("{id}/return")]
    public async Task<IActionResult> Return(int id)
    {
        var open = await _context.AssetAllocations
            .Where(aa => aa.AssetId == id && aa.ReturnedAt == null)
            .OrderByDescending(aa => aa.AllocatedAt).FirstOrDefaultAsync();
        if (open == null) return BadRequest("No active allocation found");
        open.ReturnedAt = DateTime.UtcNow;

        var asset = await _context.Assets.FindAsync(id);
        if (asset != null) asset.Status = "Available";
        await _context.SaveChangesAsync();
        return Ok();
    }

    // === MAINTENANCE ===

    [HttpGet("{id}/maintenance")]
    public async Task<IActionResult> GetMaintenance(int id)
    {
        var records = await _context.AssetMaintenances.Where(m => m.AssetId == id).OrderByDescending(m => m.ScheduledDate).ToListAsync();
        return Ok(records);
    }

    [HttpPost("{id}/maintenance")]
    public async Task<IActionResult> AddMaintenance(int id, [FromBody] AddMaintenanceRequest request)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return NotFound();

        var record = new AssetMaintenance
        {
            AssetId = id, Description = request.Description, Type = request.Type,
            Cost = request.Cost, ScheduledDate = request.ScheduledDate ?? DateTime.UtcNow, Notes = request.Notes
        };
        _context.AssetMaintenances.Add(record);
        if (request.StartMaintenance) asset.Status = "Maintenance";
        await _context.SaveChangesAsync();
        return Ok(new { record.Id });
    }

    [HttpPut("maintenance/{id}")]
    public async Task<IActionResult> UpdateMaintenance(int id, [FromBody] UpdateMaintenanceRequest request)
    {
        var record = await _context.AssetMaintenances.FindAsync(id);
        if (record == null) return NotFound();
        record.Status = request.Status;
        record.CompletedAt = request.Status == "Completed" ? DateTime.UtcNow : null;
        record.Cost = request.Cost ?? record.Cost;
        record.Notes = request.Notes ?? record.Notes;
        await _context.SaveChangesAsync();

        if (record.Status == "Completed")
        {
            var asset = await _context.Assets.FindAsync(record.AssetId);
            if (asset != null && asset.Status == "Maintenance")
            {
                var hasOpenAlloc = await _context.AssetAllocations.AnyAsync(aa => aa.AssetId == record.AssetId && aa.ReturnedAt == null);
                asset.Status = hasOpenAlloc ? "Allocated" : "Available";
                await _context.SaveChangesAsync();
            }
        }
        return Ok(record);
    }

    private static object MapAsset(Asset a)
    {
        var currentAlloc = a.Allocations?.FirstOrDefault(aa => aa.ReturnedAt == null);
        return new
        {
            a.Id, a.Name, a.AssetTag, a.Category, a.Model, a.SerialNumber,
            a.PurchaseDate, a.PurchasePrice, a.WarrantyExpiry, a.Status, a.Notes, a.CreatedAt,
            CurrentAllocation = currentAlloc != null ? new
            {
                currentAlloc.Id, currentAlloc.AllocatedAt, currentAlloc.Notes,
                EmployeeName = $"{currentAlloc.Employee.FirstName} {currentAlloc.Employee.LastName}",
                EmployeeId = currentAlloc.EmployeeId
            } : null,
            AllocationCount = a.Allocations?.Count ?? 0,
            MaintenanceCount = a.MaintenanceRecords?.Count ?? 0
        };
    }
}

public record CreateAssetRequest(string Name, string AssetTag, string Category, string? Model, string? SerialNumber, DateTime? PurchaseDate, decimal? PurchasePrice, DateTime? WarrantyExpiry, string? Notes);
public record UpdateStatusReq(string Status);
public record AllocateRequest(int EmployeeId, string? Notes);
public record AddMaintenanceRequest(string Description, string Type, decimal? Cost, DateTime? ScheduledDate, string? Notes, bool StartMaintenance = false);
public record UpdateMaintenanceRequest(string Status, decimal? Cost, string? Notes);
