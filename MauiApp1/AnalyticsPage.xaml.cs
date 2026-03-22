using Microcharts;
using SkiaSharp;
using MauiApp1.Services;

namespace MauiApp1;

public partial class AnalyticsPage : ContentPage
{
    private bool isDonut = true;

    public AnalyticsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadData();
    }

    private async Task LoadData()
    {
        var db = App.Database;
        int accountId = Preferences.Get("SelectedAccountId", 1);

        var all = await db.GetAllAsync();
        var filtered = all.Where(x => x.AccountId == accountId).ToList();

        var income = filtered.Where(x => x.Type == "Income").Sum(x => x.Amount);
        var expense = filtered.Where(x => x.Type == "Expense").Sum(x => x.Amount);

        BalanceLabel.Text = $"${income - expense:0.00}";
        IncomeLabel.Text = $"${income:0.00}";
        ExpenseLabel.Text = $"${expense:0.00}";

        var grouped = filtered
            .Where(x => x.Type == "Expense")
            .GroupBy(x => x.Category)
            .ToList();

        var entries = grouped.Select(g => new ChartEntry((float)g.Sum(x => x.Amount))
        {
            Label = "",
            ValueLabel = "",
            Color = SKColor.Parse(GetColor(g.Key))
        }).ToList();

        // 🔄 SWITCH CHART TYPE
        if (isDonut)
        {
            ChartView.Chart = new DonutChart
            {
                Entries = entries,
                HoleRadius = 0.6f,
                BackgroundColor = SKColors.Transparent
            };
        }
        else
        {
            ChartView.Chart = new BarChart
            {
                Entries = entries,
                BackgroundColor = SKColors.Transparent,
                LabelTextSize = 30
            };
        }

        // 🔥 LEGEND
        LegendLayout.Children.Clear();

        foreach (var item in grouped)
        {
            var color = GetColor(item.Key);
            var total = item.Sum(x => x.Amount);

            LegendLayout.Children.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#1E293B"),
                CornerRadius = 15,
                Padding = 10,
                Content = new HorizontalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        new BoxView
                        {
                            WidthRequest = 12,
                            HeightRequest = 12,
                            CornerRadius = 6,
                            Color = Color.FromArgb(GetColor(item.Key))
                        },
                        new Label
                        {
                            Text = item.Key,
                            TextColor = Colors.White
                        },
                        new Label
                        {
                            Text = $"${total:0}",
                            TextColor = Colors.Gray,
                            HorizontalOptions = LayoutOptions.EndAndExpand
                        }
                    }
                }
            });
        }
    }

    private async void OnSwitchChart(object sender, EventArgs e)
    {
        isDonut = !isDonut;
        await LoadData();
    }

    private static string GetColor(string category)
    {
        return category switch
        {
            "Food" => "#F59E0B",
            "Travel" => "#3B82F6",
            "Bills" => "#EF4444",
            "Entertainment" => "#10B981",
            _ => "#8B5CF6"
        };
    }
}