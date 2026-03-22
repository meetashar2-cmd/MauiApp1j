namespace MauiApp1;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("dashboard", typeof(DashboardPage));
        Routing.RegisterRoute("add", typeof(AddTransactionPage));
        Routing.RegisterRoute("analytics", typeof(AnalyticsPage));
        Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
        Routing.RegisterRoute(nameof(AnalyticsPage), typeof(AnalyticsPage));
    }
}
