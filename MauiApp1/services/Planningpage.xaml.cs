using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1;

public partial class PlanningPage : ContentPage
{
    private List<Account> accounts = new();

    public PlanningPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAccounts();
        await LoadPlansForSelectedAccount();
    }

    private async Task LoadAccounts()
    {
        accounts = await App.Database.GetAccountsAsync();

        AccountPicker.ItemsSource = accounts;
        AccountPicker.ItemDisplayBinding = new Binding("Name");

        if (!accounts.Any())
        {
            PlansList.ItemsSource = null;
            return;
        }

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

    private async Task LoadPlansForSelectedAccount()
    {
        var selectedAccount = AccountPicker.SelectedItem as Account;

        if (selectedAccount == null)
        {
            PlansList.ItemsSource = null;
            return;
        }

        Preferences.Set("SelectedAccountId", selectedAccount.Id);

        var allPlans = await App.Database.GetPlansAsync(selectedAccount.Id);
        var transactions = await App.Database.GetAllAsync();

        var plans = allPlans
            .Where(p => p.Month == DateTime.Now.Month && p.Year == DateTime.Now.Year)
            .GroupBy(p => p.Category)
            .Select(g => g.OrderByDescending(x => x.Id).First())
            .ToList();

        var planViews = plans.Select(plan =>
        {
            double spent = transactions
                .Where(t =>
                    t.AccountId == selectedAccount.Id &&
                    t.Type == "Expense" &&
                    t.Category == plan.Category &&
                    t.Date.Month == plan.Month &&
                    t.Date.Year == plan.Year)
                .Sum(t => t.Amount);

            double remaining = plan.Amount - spent;
            double progress = 0;

            if (plan.Amount > 0)
                progress = spent / plan.Amount;

            if (progress > 1)
                progress = 1;

            return new BudgetPlanView
            {
                Category = plan.Category,
                Amount = plan.Amount,
                Spent = spent,
                Remaining = remaining,
                Progress = progress
            };
        }).ToList();

        PlansList.ItemsSource = planViews;
    }

    private async void OnAccountChanged(object sender, EventArgs e)
    {
        var selectedAccount = AccountPicker.SelectedItem as Account;

        if (selectedAccount == null)
            return;

        Preferences.Set("SelectedAccountId", selectedAccount.Id);
        await LoadPlansForSelectedAccount();
    }

    private async void OnAddPlan(object sender, EventArgs e)
    {
        var selectedAccount = AccountPicker.SelectedItem as Account;

        if (selectedAccount == null)
        {
            await DisplayAlert("Error", "Please select an account", "OK");
            return;
        }

        if (CategoryPicker.SelectedItem == null || string.IsNullOrWhiteSpace(AmountEntry.Text))
        {
            await DisplayAlert("Error", "Fill all fields", "OK");
            return;
        }

        if (!double.TryParse(AmountEntry.Text, out double amount) || amount <= 0)
        {
            await DisplayAlert("Error", "Enter a valid amount", "OK");
            return;
        }

        string category = CategoryPicker.SelectedItem.ToString();

        var existingPlans = await App.Database.GetPlansAsync(selectedAccount.Id);

        bool alreadyExists = existingPlans.Any(p =>
            p.AccountId == selectedAccount.Id &&
            p.Category == category &&
            p.Month == DateTime.Now.Month &&
            p.Year == DateTime.Now.Year);

        if (alreadyExists)
        {
            await DisplayAlert(
                "Already Exists",
                $"A plan for {category} already exists for this account this month.",
                "OK");
            return;
        }

        var plan = new BudgetPlan
        {
            AccountId = selectedAccount.Id,
            Category = category,
            Amount = amount,
            Month = DateTime.Now.Month,
            Year = DateTime.Now.Year
        };

        await App.Database.AddPlanAsync(plan);

        AmountEntry.Text = string.Empty;
        CategoryPicker.SelectedItem = null;

        await LoadPlansForSelectedAccount();

        await DisplayAlert("Success", "Plan saved successfully", "OK");
    }
}

public class BudgetPlanView
{
    public string Category { get; set; }
    public double Amount { get; set; }
    public double Spent { get; set; }
    public double Remaining { get; set; }
    public double Progress { get; set; }

    public Color ProgressColor =>
        Progress >= 1 ? Colors.Red :
        Progress >= 0.8 ? Colors.Orange :
        Colors.LimeGreen;

    public Color RemainingColor =>
        Progress >= 1 ? Colors.Red :
        Progress >= 0.8 ? Colors.Orange :
        Colors.LightGreen;
}