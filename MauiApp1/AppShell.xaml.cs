namespace MauiApp1;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
        Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
        Routing.RegisterRoute(nameof(AnalyticsPage), typeof(AnalyticsPage));
    }
}