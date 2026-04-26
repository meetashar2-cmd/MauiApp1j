using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1;

public partial class PlanningPage : ContentPage
{

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
            Month = DateTime.Now.Month,
            Year = DateTime.Now.Year
        };

        await App.Database.AddPlanAsync(plan);


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