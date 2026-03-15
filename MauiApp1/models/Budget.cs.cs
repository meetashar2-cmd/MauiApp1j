// Models/Budget.cs
namespace MauiApp1;

public static class Budget
{
    // Monthly caps per category (USD). Edit these values or add categories as you like.
    public static readonly Dictionary<string, decimal> Caps = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Food"] = 300m,
        ["Transport"] = 120m,
        ["Shopping"] = 250m,
        ["Entertainment"] = 150m,
        ["Other"] = 100m
    };
}