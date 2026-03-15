using SQLite;

namespace MauiApp1;

public class TransactionService
{
    private SQLiteAsyncConnection? _db;

    public async Task InitAsync()
    {
        if (_db != null) return;

        var path = Path.Combine(FileSystem.AppDataDirectory, "budget.db3");
        _db = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        await _db.CreateTableAsync<Transaction>();
    }

    public async Task<List<Transaction>> GetAllAsync()
    {
        await InitAsync();
        return await _db!.Table<Transaction>()
                         .OrderByDescending(t => t.Date)
                         .ThenByDescending(t => t.Id)
                         .ToListAsync();
    }

    public async Task SaveAsync(Transaction t)
    {
        await InitAsync();
        if (t.Id != 0) await _db!.UpdateAsync(t);
        else await _db!.InsertAsync(t);
    }

    public async Task DeleteAsync(Transaction t)
    {
        await InitAsync();
        await _db!.DeleteAsync(t);
    }

    // Example aggregation using DECIMAL everywhere
    public async Task<(decimal income, decimal expenses, decimal balance)> GetTotalsAsync()
    {
        var all = await GetAllAsync();

        decimal income = 0m;
        decimal expenses = 0m;

        foreach (var t in all)
        {
            if (t.Type == "Income")
                income += t.Amount;      // decimal += decimal
            else if (t.Type == "Expense")
                expenses += t.Amount;    // decimal += decimal
        }

        var balance = income - expenses;
        return (income, expenses, balance);
    }

    // Example ratio (if you really need double for a chart, cast ONCE)
    public static double ToRatio(decimal part, decimal total)
        => total <= 0m ? 0.0 : (double)(part / total);
}