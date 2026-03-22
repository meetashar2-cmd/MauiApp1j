using LiveChartsCore.SkiaSharpView.Maui;
using MauiApp1.Services;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MauiApp1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .UseLiveCharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // ✅ FIXED POSITION (IMPORTANT)
        builder.Services.AddSingleton<ITransactionDatabase, TransactionDatabase>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}