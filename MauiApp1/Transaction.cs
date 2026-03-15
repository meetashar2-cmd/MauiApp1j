using SQLite;

namespace MauiApp1;

public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    // Store absolute amount (Type determines meaning)
    public decimal Amount { get; set; }

    // "Income" or "Expense"
    public string Type { get; set; } = "Expense";

    public string Category { get; set; } = "Other";

    public DateTime Date { get; set; } = DateTime.Today;
}