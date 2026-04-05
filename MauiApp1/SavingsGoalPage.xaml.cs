using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1;

public partial class SavingsGoalPage : ContentPage
{
    private List<Account> accounts = new();

    public SavingsGoalPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAccounts();
        await LoadGoalsForSelectedAccount();
    }

    private async Task LoadAccounts()
    {
        accounts = await App.Database.GetAccountsAsync();

        AccountPicker.ItemsSource = accounts;
        AccountPicker.ItemDisplayBinding = new Binding("Name");

        if (!accounts.Any())
        {
            GoalsList.ItemsSource = null;
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

    private async Task LoadGoalsForSelectedAccount()
    {
        var selectedAccount = AccountPicker.SelectedItem as Account;

        if (selectedAccount == null)
        {
            GoalsList.ItemsSource = null;
            return;
        }

        Preferences.Set("SelectedAccountId", selectedAccount.Id);

        var goals = await App.Database.GetSavingsGoalsAsync(selectedAccount.Id);

        var goalViews = goals.Select(goal =>
        {
            double progress = 0;

            if (goal.TargetAmount > 0)
                progress = goal.SavedAmount / goal.TargetAmount;

            if (progress > 1)
                progress = 1;

            return new SavingsGoalView
            {
                Id = goal.Id,
                AccountId = goal.AccountId,
                GoalName = goal.GoalName,
                TargetAmount = goal.TargetAmount,
                SavedAmount = goal.SavedAmount,
                Remaining = goal.TargetAmount - goal.SavedAmount,
                Progress = progress,
                ProgressText = $"{progress:P0}"
            };
        }).ToList();

        GoalsList.ItemsSource = goalViews;
    }

    private async void OnAccountChanged(object sender, EventArgs e)
    {
        var selectedAccount = AccountPicker.SelectedItem as Account;

        if (selectedAccount == null)
            return;

        Preferences.Set("SelectedAccountId", selectedAccount.Id);
        await LoadGoalsForSelectedAccount();
    }

    private async void OnSaveGoalClicked(object sender, EventArgs e)
    {
        var selectedAccount = AccountPicker.SelectedItem as Account;

        if (selectedAccount == null)
        {
            await DisplayAlert("Error", "Please select an account", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(GoalNameEntry.Text) ||
            string.IsNullOrWhiteSpace(TargetAmountEntry.Text) ||
            string.IsNullOrWhiteSpace(SavedAmountEntry.Text))
        {
            await DisplayAlert("Error", "Fill all fields", "OK");
            return;
        }

        if (!double.TryParse(TargetAmountEntry.Text, out double targetAmount) || targetAmount <= 0)
        {
            await DisplayAlert("Error", "Enter a valid target amount", "OK");
            return;
        }

        if (!double.TryParse(SavedAmountEntry.Text, out double savedAmount) || savedAmount < 0)
        {
            await DisplayAlert("Error", "Enter a valid saved amount", "OK");
            return;
        }

        var goal = new SavingsGoal
        {
            AccountId = selectedAccount.Id,
            GoalName = GoalNameEntry.Text.Trim(),
            TargetAmount = targetAmount,
            SavedAmount = savedAmount
        };

        await App.Database.AddSavingsGoalAsync(goal);

        GoalNameEntry.Text = string.Empty;
        TargetAmountEntry.Text = string.Empty;
        SavedAmountEntry.Text = string.Empty;

        await LoadGoalsForSelectedAccount();

        await DisplayAlert("Success", "Savings goal saved successfully", "OK");
    }
}

public class SavingsGoalView
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string GoalName { get; set; }
    public double TargetAmount { get; set; }
    public double SavedAmount { get; set; }
    public double Remaining { get; set; }
    public double Progress { get; set; }
    public string ProgressText { get; set; }
}