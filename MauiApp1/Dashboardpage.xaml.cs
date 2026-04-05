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
            int selectedAccountId = Preferences.Get("SelectedAccountId", accounts.First().Id);

            var selectedAccount = accounts.FirstOrDefault(a => a.Id == selectedAccountId);

            if (selectedAccount != null)
            {
                AccountPicker.SelectedItem = selectedAccount;
            }
            else
            {
                AccountPicker.SelectedIndex = 0;
                Preferences.Set("SelectedAccountId", accounts[0].Id);
            }
        }
    }

    // =========================
    // LOAD DATA
    // =========================
    private async Task LoadData()
    {
        int accountId = Preferences.Get("SelectedAccountId", 1);

        var all = await App.Database.GetAllAsync();
        var filtered = all.Where(x => x.AccountId == accountId).ToList();

        double income = filtered
            .Where(x => x.Type == "Income")
            .Sum(x => x.Amount);

        double expense = filtered
            .Where(x => x.Type == "Expense")
            .Sum(x => x.Amount);

        BalanceLabel.Text = $"${income - expense:0.00}";
        IncomeLabel.Text = $"${income:0.00}";
        ExpenseLabel.Text = $"${expense:0.00}";

        await LoadInsights(filtered, accountId, income, expense);
    }

    // =========================
    // SMART INSIGHTS
    // =========================
    private async Task LoadInsights(List<TransactionRecord> filtered, int accountId, double income, double expense)
    {
        if (!filtered.Any())
        {
            TopCategoryLabel.Text = "No transactions yet.";
            BalanceStatusLabel.Text = "";
            BudgetStatusLabel.Text = "";
            return;
        }

        // TOP CATEGORY
        var expenseGroups = filtered
            .Where(x => x.Type == "Expense")
            .GroupBy(x => x.Category)
            .OrderByDescending(g => g.Sum(x => x.Amount))
            .ToList();

        string topCategory = expenseGroups.Any()
            ? expenseGroups.First().Key
            : "None";

        double topAmount = expenseGroups.Any()
            ? expenseGroups.First().Sum(x => x.Amount)
            : 0;

        TopCategoryLabel.Text = $"Top spending: {topCategory} (${topAmount:0.00})";

        // BALANCE STATUS
        if (income >= expense)
        {
            BalanceStatusLabel.Text = "You are saving money this month 👍";
            BalanceStatusLabel.TextColor = Colors.LightGreen;
        }
        else
        {
            BalanceStatusLabel.Text = "You are overspending this month ⚠️";
            BalanceStatusLabel.TextColor = Colors.Orange;
        }

        // BUDGET STATUS
        var plans = await App.Database.GetPlansAsync(accountId);

        string budgetMessage = "All budgets are under control ✅";
        Color budgetColor = Colors.LightGreen;

        var currentMonthPlans = plans
            .Where(p => p.Month == DateTime.Now.Month && p.Year == DateTime.Now.Year)
            .ToList();

        foreach (var plan in currentMonthPlans)
        {
            double spent = filtered
                .Where(t =>
                    t.Type == "Expense" &&
                    t.Category == plan.Category &&
                    t.Date.Month == DateTime.Now.Month &&
                    t.Date.Year == DateTime.Now.Year)
                .Sum(t => t.Amount);

            if (plan.Amount > 0)
            {
                double percentage = spent / plan.Amount;

                if (percentage >= 1)
                {
                    budgetMessage = $"Exceeded {plan.Category} budget 🚨";
                    budgetColor = Colors.Red;
                    break;
                }
                else if (percentage >= 0.8)
                {
                    budgetMessage = $"Near {plan.Category} budget ⚠️";
                    budgetColor = Colors.Orange;
                }
            }
        }

        BudgetStatusLabel.Text = budgetMessage;
        BudgetStatusLabel.TextColor = budgetColor;
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
                Name = name.Trim()
            });

            await LoadAccounts();
            await LoadData();
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

    private async void OnSavingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SavingsGoalPage());
    }
}