using SQLite;

namespace MauiApp1.Models;

public class SavingsGoal
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int AccountId { get; set; }

    public string GoalName { get; set; }

    public double TargetAmount { get; set; }

    public double SavedAmount { get; set; }
}