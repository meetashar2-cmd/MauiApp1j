using SQLite;

namespace MauiApp1;

public static class Db
{
    static SQLiteAsyncConnection? _db;

    static async Task Init()
    {
        if (_db != null) return;

        var path = Path.Combine(FileSystem.AppDataDirectory, "data.db3");
        _db = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        await _db.CreateTableAsync<Transaction>();
    }

    public static async Task<List<Transaction>> GetAll()
    {
        await Init();
        return await _db!.Table<Transaction>()
                         .OrderByDescending(x => x.Date)
                         .ThenByDescending(x => x.Id)
                         .ToListAsync();
    }

    public static async Task<Transaction?> GetById(int id)
    {
        await Init();
        return await _db!.Table<Transaction>()
                         .Where(t => t.Id == id)
                         .FirstOrDefaultAsync();
    }

    public static async Task Save(Transaction t)
    {
        await Init();
        if (t.Id != 0) await _db!.UpdateAsync(t);
        else await _db!.InsertAsync(t);
    }

    public static async Task Delete(Transaction t)
    {
        await Init();
        await _db!.DeleteAsync(t);
    }
    }