namespace HRMS.API.Models.Entities;

public class ExpenseReport
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Paid
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int? ReviewedById { get; set; }
    public Employee? ReviewedBy { get; set; }
    public string? ReviewNotes { get; set; }
    public ICollection<ExpenseLineItem> LineItems { get; set; } = new List<ExpenseLineItem>();
}
