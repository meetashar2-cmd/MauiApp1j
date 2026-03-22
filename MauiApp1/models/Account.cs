using SQLite;

namespace MauiApp1.Models;

public class Account
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";
}