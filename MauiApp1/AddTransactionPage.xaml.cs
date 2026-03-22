using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1;

public partial class AddTransactionPage : ContentPage
{
    private readonly ITransactionDatabase _db;

    public AddTransactionPage()
    {
        InitializeComponent();

        _db = IPlatformApplication.Current.Services.GetService<ITransactionDatabase>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_db == null)
            return;

        var accounts = await _db.GetAccountsAsync();

        AccountPicker.ItemsSource = accounts;
        AccountPicker.ItemDisplayBinding = new Binding("Name");

        if (accounts.Any())
            AccountPicker.SelectedIndex = 0;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_db == null)
        {
            await DisplayAlert("Error", "Database not available", "OK");
            return;
        }

        if (!double.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
        {
            await DisplayAlert("Validation", "Enter a valid amount", "OK");
            return;
        }

        var selectedAccount = AccountPicker.SelectedItem as Account;

        if (selectedAccount == null)
        {
            await DisplayAlert("Validation", "Please select an account", "OK");
            return;
        }

        var tx = new TransactionRecord
        {
            Title = TitleEntry.Text?.Trim() ?? "No Title",
            Amount = amount,
            Category = CategoryPicker.SelectedItem?.ToString() ?? "Other",
            Type = TypePicker.SelectedItem?.ToString() ?? "Expense",
            Date = DatePicker.Date ?? DateTime.Now,
            AccountId = selectedAccount.Id
        };

        await _db.AddAsync(tx);

        if (tx.Type == "Expense")
        {
            await CheckBudgetStatus(tx);
        }

        await DisplayAlert("Success", "Transaction saved!", "OK");
        await Shell.Current.GoToAsync("//dashboard");
    }

    private async Task CheckBudgetStatus(TransactionRecord tx)
    {
        if (_db == null)
            return;

        var plans = await _db.GetPlansAsync(tx.AccountId);

        var matchingPlan = plans.FirstOrDefault(p =>
            p.Category == tx.Category &&
            p.Month == tx.Date.Month &&
            p.Year == tx.Date.Year);

        if (matchingPlan == null || matchingPlan.Amount <= 0)
            return;

        var allTransactions = await _db.GetAllAsync();

        var totalSpent = allTransactions
            .Where(t =>
                t.AccountId == tx.AccountId &&
                t.Type == "Expense" &&
                t.Category == tx.Category &&
                t.Date.Month == tx.Date.Month &&
                t.Date.Year == tx.Date.Year)
            .Sum(t => t.Amount);

        double percentage = (totalSpent / matchingPlan.Amount) * 100;

        if (percentage >= 100)
        {
            await DisplayAlert(
                "🚨 Budget Exceeded",
                $"You exceeded your {tx.Category} budget!\n\nBudget: ${matchingPlan.Amount:F2}\nSpent: ${totalSpent:F2}",
                "OK");
        }
        else if (percentage >= 80)
        {
            await DisplayAlert(
                "⚠️ Budget Warning",
                $"You’ve used {percentage:F0}% of your {tx.Category} budget.\n\nBudget: ${matchingPlan.Amount:F2}\nSpent: ${totalSpent:F2}",
                "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//dashboard");
    }
}