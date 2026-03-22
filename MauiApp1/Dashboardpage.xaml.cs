using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await App.Database.EnsureDefaultAccountAsync();
        await LoadAccounts();
        await LoadData();
    }

    // =========================
    // LOAD ACCOUNTS
    // =========================
    private async Task LoadAccounts()
    {
        var accounts = await App.Database.GetAccountsAsync();

        AccountPicker.ItemsSource = accounts;
        AccountPicker.ItemDisplayBinding = new Binding("Name");

        if (accounts.Any())
        {
            AccountPicker.SelectedIndex = 0;
        }
    }

    // =========================
    // LOAD DATA
    // =========================
    private async Task LoadData()
    {
        int accountId = Preferences.Get("SelectedAccountId", 1);

        var all = await App.Database.GetAllAsync();

        var filtered = all.Where(x => x.AccountId == accountId);

        double income = filtered
            .Where(x => x.Type == "Income")
            .Sum(x => x.Amount);

        double expense = filtered
            .Where(x => x.Type == "Expense")
            .Sum(x => x.Amount);

        BalanceLabel.Text = $"${income - expense:0.00}";
        IncomeLabel.Text = $"${income:0.00}";
        ExpenseLabel.Text = $"${expense:0.00}";
    }

    // =========================
    // ACCOUNT CHANGED
    // =========================
    private async void OnAccountChanged(object sender, EventArgs e)
    {
        var selected = AccountPicker.SelectedItem as Account;

        if (selected == null)
            return;

        Preferences.Set("SelectedAccountId", selected.Id);

        await LoadData();
    }

    // =========================
    // ADD ACCOUNT
    // =========================
    private async void OnAddAccountClicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("New Account", "Enter account name:");

        if (!string.IsNullOrWhiteSpace(name))
        {
            await App.Database.AddAccountAsync(new Account
            {
                Name = name
            });

            await LoadAccounts();
        }
    }

    // =========================
    // NAVIGATION
    // =========================
    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddTransactionPage());
    }

    private async void OnAnalyticsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AnalyticsPage());
    }

    private async void OnPlanningClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PlanningPage());
    }
}