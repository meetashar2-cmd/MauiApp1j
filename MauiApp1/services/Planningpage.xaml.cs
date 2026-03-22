using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1;

public partial class PlanningPage : ContentPage
{
    private List<BudgetPlan> plans = new();

    public PlanningPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var db = App.Database;
        int accountId = Preferences.Get("SelectedAccountId", 1);
        plans = await db.GetPlansAsync(accountId); 

        PlansList.ItemsSource = plans;
    }

    private async void OnAddPlan(object sender, EventArgs e)
    {
        if (CategoryPicker.SelectedItem == null || string.IsNullOrWhiteSpace(AmountEntry.Text))
        {
            await DisplayAlert("Error", "Fill all fields", "OK");
            return;
        }

        var plan = new BudgetPlan
        {
            Category = CategoryPicker.SelectedItem.ToString(),
            Amount = double.Parse(AmountEntry.Text),
            Month = DateTime.Now.Month,
            Year = DateTime.Now.Year
        };

        await App.Database.AddPlanAsync(plan);

        plans.Add(plan);
        PlansList.ItemsSource = null;
        PlansList.ItemsSource = plans;

        AmountEntry.Text = "";
    }
}