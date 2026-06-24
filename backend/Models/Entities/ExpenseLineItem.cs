namespace HRMS.API.Models.Entities;

public class ExpenseLineItem
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty; // Travel, Food, Office, Equipment, Other
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? ReceiptPath { get; set; }

    public int ExpenseReportId { get; set; }
    public ExpenseReport ExpenseReport { get; set; } = null!;
}
