using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1;

public partial class AddTransactionPage : ContentPage
{
    private readonly ITransactionDatabase _db;

    public AddTransactionPage()
    {
        InitializeComponent();
        TypePicker.SelectedIndex = 1;     // default Expense
        CategoryPicker.SelectedIndex = 0; // default first category
        DatePicker.Date = DateTime.Today;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_db == null)
        {
            await DisplayAlert("Error", "Database not available", "OK");
            return;
        }

        if (!decimal.TryParse(AmountEntry.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var rawAmount))
        {
            await DisplayAlert("Validation", "Enter a valid amount", "OK");
            return;
        }

        var selectedType = TypePicker.SelectedIndex == 0 ? "Income" : "Expense";
        // Normalize: store absolute amount; type decides semantic sign
        var amount = Math.Abs(rawAmount);

        var t = new Transaction
        {
            Title = TitleEntry.Text?.Trim() ?? "No Title",
            Amount = amount,
            Category = CategoryPicker.SelectedItem?.ToString() ?? "Other",
            Type = TypePicker.SelectedItem?.ToString() ?? "Expense",
            Date = DatePicker.Date ?? DateTime.Now,
            AccountId = selectedAccount.Id
        };

        await Db.Save(t);
        await DisplayAlert("Saved", "Transaction saved.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}