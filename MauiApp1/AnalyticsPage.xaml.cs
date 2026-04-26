using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using MauiApp1.Services;

namespace MauiApp1;

public partial class AnalyticsPage : ContentPage
{
    private bool isDonut = true;

    public AnalyticsPage()
    {
        InitializeComponent();
        BindingContext = this;

        LoadDemoData();
    }

    void LoadDemoData()
    {
        CategoryPieSeries = new ISeries[]
        {
            new PieSeries<double>{ Values = new double[]{200}, Name="Shopping"},
            new PieSeries<double>{ Values = new double[]{120}, Name="Food"},
            new PieSeries<double>{ Values = new double[]{30}, Name="Other"}
        };

        DailyLineSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = new double[] { 10, 50, 30, 80, 20, 90, 60 },
                Stroke = new SolidColorPaint(SKColors.MediumPurple){StrokeThickness=3}
            }
        };

        DailyXAxes = new Axis[]
        {
            new Axis { Labels = new [] { "Mon","Tue","Wed","Thu","Fri","Sat","Sun" } }
        };

        DailyYAxes = new Axis[]
        {
            new Axis { Labeler = v => "$" + v.ToString() }
        };

        TotalExpensesLabel.Text = "$350";
        TopCategoryLabel.Text = "Shopping";

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