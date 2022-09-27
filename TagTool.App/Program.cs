﻿using Avalonia;
using Serilog;
using Serilog.Core.Enrichers;
using Serilog.Events;

namespace TagTool.App;

public static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    private static AppBuilder BuildAvaloniaApp()
    {
        SetupSerilog();

        var appBuilder = AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

        return appBuilder;
    }

    private static void SetupSerilog()
    {
        var logsDbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TagToolBackend",
            "Logs",
            "applog.db");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.With(new PropertyEnricher("ApplicationName", "TagToolApp"))
            .WriteTo.SQLite(logsDbPath, storeTimestampInUtc: true, batchSize: 1)
            .CreateLogger();
    }
}
