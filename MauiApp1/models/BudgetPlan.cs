using SQLite;

namespace MauiApp1.Models;

public class BudgetPlan
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Category { get; set; }
    public double Amount { get; set; }

    public int Month { get; set; }   // 🔥 ADD THIS
    public int Year { get; set; }    // 🔥 ADD THIS

    public int AccountId { get; set; } // 🔥 REQUIRED
}