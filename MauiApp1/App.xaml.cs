using MauiApp1.Services;

namespace MauiApp1;

public partial class App : Application
{
    // ✅ GLOBAL DATABASE
    public static ITransactionDatabase Database { get; private set; }

    public App(ITransactionDatabase db)
    {
        InitializeComponent();

        MainPage = new NavigationPage(new AnalyticsPage());
    }
}