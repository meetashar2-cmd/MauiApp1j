namespace MauiApp1.Models;

public class TransactionRecord
{
    public int AccountId { get; set; }
    public string Title { get; set; }
    public double Amount { get; set; }
    public string Category { get; set; }
    public string Type { get; set; } // Income / Expense
    public DateTime Date { get; set; }
}