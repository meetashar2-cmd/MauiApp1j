using SQLite;
using MauiApp1.Models;

namespace MauiApp1.Services;

public interface ITransactionDatabase
{
    // ================= TRANSACTIONS =================
    Task<List<TransactionRecord>> GetAllAsync();
    Task<List<TransactionRecord>> GetRecentAsync();
    Task<(double income, double expense)> GetSummaryAsync();
    Task<Dictionary<string, double>> GetCategoryTotalsAsync();

    Task<int> AddAsync(TransactionRecord tx);
    Task<int> UpdateAsync(TransactionRecord tx);
    Task<int> DeleteAsync(TransactionRecord tx);

    // ================= ACCOUNTS =================
    Task<List<Account>> GetAccountsAsync();
    Task<int> AddAccountAsync(Account acc);
    Task EnsureDefaultAccountAsync();

    // ================= PLANNING =================
    Task<int> AddPlanAsync(BudgetPlan plan);
    Task<List<BudgetPlan>> GetPlansAsync(int accountId);
    Task<int> DeletePlanAsync(BudgetPlan plan);

    // ================= SAVINGS GOALS =================
    Task<int> AddSavingsGoalAsync(SavingsGoal goal);
    Task<int> UpdateSavingsGoalAsync(SavingsGoal goal);
    Task<int> DeleteSavingsGoalAsync(SavingsGoal goal);
    Task<List<SavingsGoal>> GetSavingsGoalsAsync(int accountId);
}

public class TransactionDatabase : ITransactionDatabase
{
    private readonly SQLiteAsyncConnection _db;

    public TransactionDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "transactions.db");
        _db = new SQLiteAsyncConnection(dbPath);

        _db.CreateTableAsync<TransactionRecord>().Wait();
        _db.CreateTableAsync<Account>().Wait();
        _db.CreateTableAsync<BudgetPlan>().Wait();
        _db.CreateTableAsync<SavingsGoal>().Wait(); // ✅ NEW TABLE
    }

    // ================= TRANSACTIONS =================

    public async Task<List<TransactionRecord>> GetAllAsync()
    {
        return await _db.Table<TransactionRecord>().ToListAsync();
    }

    public async Task<List<TransactionRecord>> GetRecentAsync()
    {
        return await _db.Table<TransactionRecord>()
            .OrderByDescending(x => x.Date)
            .Take(10)
            .ToListAsync();
    }

    public async Task<(double income, double expense)> GetSummaryAsync()
    {
        var all = await _db.Table<TransactionRecord>().ToListAsync();

        double income = all.Where(x => x.Type == "Income").Sum(x => x.Amount);
        double expense = all.Where(x => x.Type == "Expense").Sum(x => x.Amount);

        return (income, expense);
    }

    public async Task<Dictionary<string, double>> GetCategoryTotalsAsync()
    {
        var list = await _db.Table<TransactionRecord>().ToListAsync();

        return list
            .Where(x => x.Type == "Expense")
            .GroupBy(x => x.Category)
            .ToDictionary(
                g => g.Key ?? "Other",
                g => g.Sum(x => x.Amount)
            );
    }

    public async Task<int> AddAsync(TransactionRecord tx)
    {
        return await _db.InsertAsync(tx);
    }

    public async Task<int> UpdateAsync(TransactionRecord tx)
    {
        return await _db.UpdateAsync(tx);
    }

    public async Task<int> DeleteAsync(TransactionRecord tx)
    {
        return await _db.DeleteAsync(tx);
    }

    // ================= ACCOUNTS =================

    public async Task<List<Account>> GetAccountsAsync()
    {
        return await _db.Table<Account>().ToListAsync();
    }

    public async Task<int> AddAccountAsync(Account acc)
    {
        return await _db.InsertAsync(acc);
    }

    public async Task EnsureDefaultAccountAsync()
    {
        var accounts = await GetAccountsAsync();

        if (accounts == null || !accounts.Any())
        {
            await AddAccountAsync(new Account { Name = "Cash" });
        }
    }

    // ================= PLANNING =================

    public async Task<int> AddPlanAsync(BudgetPlan plan)
    {
        return await _db.InsertAsync(plan);
    }

    public async Task<List<BudgetPlan>> GetPlansAsync(int accountId)
    {
        return await _db.Table<BudgetPlan>()
            .Where(x => x.AccountId == accountId)
            .ToListAsync();
    }

    public async Task<int> DeletePlanAsync(BudgetPlan plan)
    {
        return await _db.DeleteAsync(plan);
    }

    // ================= SAVINGS GOALS =================

    public async Task<int> AddSavingsGoalAsync(SavingsGoal goal)
    {
        return await _db.InsertAsync(goal);
    }

    public async Task<int> UpdateSavingsGoalAsync(SavingsGoal goal)
    {
        return await _db.UpdateAsync(goal);
    }

    public async Task<int> DeleteSavingsGoalAsync(SavingsGoal goal)
    {
        return await _db.DeleteAsync(goal);
    }

    public async Task<List<SavingsGoal>> GetSavingsGoalsAsync(int accountId)
    {
        return await _db.Table<SavingsGoal>()
            .Where(x => x.AccountId == accountId)
            .ToListAsync();
    }
}