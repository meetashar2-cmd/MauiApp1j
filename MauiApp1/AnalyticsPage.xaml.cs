using MauiApp1.Models;
using MauiApp1.Services;
using Microcharts;
using Microsoft.Maui.Controls.Shapes;
using SkiaSharp;

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

        double income = filtered
            .Where(x => x.Type == "Income")
            .Sum(x => x.Amount);

        double expense = filtered
            .Where(x => x.Type == "Expense")
            .Sum(x => x.Amount);

        BalanceLabel.Text = $"${income - expense:0.00}";
        IncomeLabel.Text = $"${income:0.00}";
        ExpenseLabel.Text = $"${expense:0.00}";

        var grouped = filtered
            .Where(x => x.Type == "Expense")
            .GroupBy(x => x.Category)
            .OrderByDescending(g => g.Sum(x => x.Amount))
            .ToList();

        LegendLayout.Children.Clear();
        ModernBarLayout.Children.Clear();

        if (!grouped.Any())
        {
            DonutChartView.Chart = new DonutChart
            {
                Entries = new List<ChartEntry>
                {
                    new ChartEntry(1)
                    {
                        Label = "",
                        ValueLabel = "",
                        Color = SKColor.Parse("#334155")
                    }
                },
                HoleRadius = 0.6f,
                BackgroundColor = SKColors.Transparent,
                LabelTextSize = 0
            };

            DonutChartView.IsVisible = true;
            ModernBarLayout.IsVisible = false;
            SwitchButton.Text = "Bar";

            LegendLayout.Children.Add(new Label
            {
                Text = "No expense data yet",
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.Center
            });

            return;
        }

        var donutEntries = grouped.Select(g => new ChartEntry((float)g.Sum(x => x.Amount))
        {
            Label = "",
            ValueLabel = "",
            Color = SKColor.Parse(GetColor(g.Key))
        }).ToList();

        DonutChartView.Chart = new DonutChart
        {
            Entries = donutEntries,
            HoleRadius = 0.6f,
            BackgroundColor = SKColors.Transparent,
            LabelTextSize = 0
        };

        if (isDonut)
        {
            DonutChartView.IsVisible = true;
            ModernBarLayout.IsVisible = false;
            SwitchButton.Text = "Bar";
        }
        else
        {
            DonutChartView.IsVisible = false;
            ModernBarLayout.IsVisible = true;
            SwitchButton.Text = "Pie";

            RenderModernBars(grouped);
        }

        foreach (var item in grouped)
        {
            string color = GetColor(item.Key);
            double total = item.Sum(x => x.Amount);

            LegendLayout.Children.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#1E293B"),
                CornerRadius = 15,
                Padding = 12,
                HasShadow = false,
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
                            Color = Color.FromArgb(color),
                            VerticalOptions = LayoutOptions.Center
                        },
                        new Label
                        {
                            Text = item.Key,
                            TextColor = Colors.White,
                            VerticalOptions = LayoutOptions.Center
                        },
                        new Label
                        {
                            Text = $"${total:0}",
                            TextColor = Colors.Gray,
                            HorizontalOptions = LayoutOptions.EndAndExpand,
                            VerticalOptions = LayoutOptions.Center
                        }
                    }
                }
            });
        }
    }

    private void RenderModernBars(List<IGrouping<string, TransactionRecord>> grouped)
    {
        ModernBarLayout.Children.Clear();

        double max = grouped.Max(g => g.Sum(x => x.Amount));
        const double barMaxWidth = 280;

        foreach (var item in grouped)
        {
            double total = item.Sum(x => x.Amount);
            double progress = max > 0 ? total / max : 0;
            string color = GetColor(item.Key);

            var iconFrame = new Frame
            {
                WidthRequest = 34,
                HeightRequest = 34,
                CornerRadius = 17,
                Padding = 0,
                HasShadow = false,
                BackgroundColor = Color.FromArgb("#22314A"),
                Content = new Label
                {
                    Text = GetIcon(item.Key),
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };

            var infoStack = new VerticalStackLayout
            {
                Spacing = 2,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = item.Key,
                        TextColor = Colors.White,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16
                    },
                    new Label
                    {
                        Text = $"Used {progress:P0} of top category",
                        TextColor = Color.FromArgb("#8FA3BF"),
                        FontSize = 12
                    }
                }
            };

            var amountFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#1F1636"),
                CornerRadius = 14,
                Padding = new Thickness(12, 6),
                HasShadow = false,
                VerticalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = $"${total:0}",
                    TextColor = Color.FromArgb("#C4B5FD"),
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 13
                }
            };

            var topGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            Grid.SetColumn(iconFrame, 0);
            Grid.SetColumn(infoStack, 1);
            Grid.SetColumn(amountFrame, 2);

            topGrid.Children.Add(iconFrame);
            topGrid.Children.Add(infoStack);
            topGrid.Children.Add(amountFrame);

            var barFill = new Border
            {
                BackgroundColor = Color.FromArgb(color),
                StrokeThickness = 0,
                HeightRequest = 14,
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = Math.Max(18, progress * barMaxWidth),
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(7)
                }
            };

            var barTrack = new Border
            {
                BackgroundColor = Color.FromArgb("#0F172A"),
                StrokeThickness = 0,
                HeightRequest = 14,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(7)
                },
                Content = barFill
            };

            var cardStack = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    topGrid,
                    barTrack
                }
            };

            ModernBarLayout.Children.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#162338"),
                BorderColor = Color.FromArgb("#22314A"),
                CornerRadius = 20,
                Padding = 16,
                HasShadow = false,
                Content = cardStack
            });
        }
    }

    private async void OnSwitchChart(object sender, EventArgs e)
    {
        isDonut = !isDonut;
        await LoadData();
    }

    private static string GetIcon(string category)
    {
        return category switch
        {
            "Food" => "🍔",
            "Travel" => "✈️",
            "Bills" => "💡",
            "Entertainment" => "🎮",
            _ => "💸"
        };
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