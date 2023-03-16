using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Model.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core.Enrichers;
using Serilog.Events;
using TagTool.App.Core.Services;
using TagTool.App.Models;
using TagTool.App.Models.Docks;
using TagTool.App.Options;
using TagTool.App.ViewModels;
using TagTool.App.Views;

namespace TagTool.App;

public class App : Application
{
    public static new App Current => (App)Application.Current!;

    public IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Services = ConfigureServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            var mainWindow = new MainWindow { DataContext = Services.GetRequiredService<MainWindowViewModel>() };

            desktopLifetime.MainWindow = mainWindow;

            desktopLifetime.Exit += (_, _) => Log.CloseAndFlush();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        var configuration = CreateConfiguration();

        SetupSerilog();

        services.AddLogging(x =>
        {
            x.SetMinimumLevel(LogLevel.Trace);
            x.AddSerilog(Log.Logger, dispose: true);
        });

        services.AddSingleton(configuration);
        services.AddSingleton<IFileIconProvider, DefaultFileIconProvider>();
        services.AddSingleton<IWordHighlighter, WordHighlighter>();
        services.AddSingleton<ITagToolBackendConnectionFactory, GrpcChannelFactory>();
        services.AddSingleton<ITagToolBackend, TagToolBackend>();

        services.AddSingleton<MyDockFactory>();
        services.AddTransient<MyDocumentDock>();

        services
            .AddOptions<GeneralOptions>()
            .Configure(options => configuration.GetSection(GeneralOptions.General).Bind(options));

        services.AddViewModels(typeof(ViewModelBase));

        return services.BuildServiceProvider();
    }

    private static IConfiguration CreateConfiguration()
        => new ConfigurationBuilder()
            .AddJsonFile("defaultAppSettings.json", optional: false, reloadOnChange: true)
            .Build();

    private static void SetupSerilog()
    {
        // var logsDbPath = Path.Combine(
        //     Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        //     "TagToolBackend",
        //     "Logs",
        //     "applog.db");

        const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.With(new PropertyEnricher("ApplicationName", "TagToolApp"))
            .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture, outputTemplate: outputTemplate)
            .WriteTo.File(
                @"C:\Users\tczyz\Documents\TagToolApp\TagToolAppLogs.txt",
                outputTemplate: outputTemplate,
                formatProvider: CultureInfo.InvariantCulture,
                shared: true)
            // .WriteTo.SQLite(logsDbPath, storeTimestampInUtc: true, batchSize: 10, formatProvider: CultureInfo.CurrentCulture)
            .CreateLogger();
    }
}

public static class ServiceCollectionExtensions
{
    private static Func<Type, bool> ViewModelBasePredicate
        => x => typeof(ViewModelBase).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false };

    private static Func<Type, bool> DockablePredicate
        => x => typeof(IDockable).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false };

    public static void AddViewModels(this IServiceCollection services, Type scanMarker)
    {
        var viewModelsBases = scanMarker.Assembly.ExportedTypes.Where(ViewModelBasePredicate);
        var dockables = scanMarker.Assembly.ExportedTypes.Where(DockablePredicate);

        foreach (var type in viewModelsBases.Union(dockables))
        {
            services.AddTransient(type);
        }
    }
}
