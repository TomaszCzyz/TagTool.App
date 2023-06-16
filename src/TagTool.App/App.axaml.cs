using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Model.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI.GPT3;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.Managers;
using Serilog;
using Serilog.Core.Enrichers;
using Serilog.Events;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.App.Docks;
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

        services.AddOptions<OpenAiOptions>().Configure(settings =>
        {
            settings.ApiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? throw new InvalidOperationException();
            settings.DefaultModelId = OpenAI.GPT3.ObjectModels.Models.ChatGpt3_5Turbo;
        });
        services.AddHttpClient<IOpenAIService, OpenAIService>();

        services.AddSingleton(configuration);
        services.AddSingleton<IFileIconProvider, DefaultFileIconProvider>();
        services.AddSingleton<IWordHighlighter, WordHighlighter>();
        services.AddSingleton<ITagToolBackendConnectionFactory, GrpcChannelFactory>();
        services.AddSingleton<ITagToolBackend, TagToolBackend>();
        services.AddTransient<ISpeechToTagSearchService, SpeechToTagSearchService>();

        services.AddSingleton<MyDockFactory>();
        services.AddTransient<MyDocumentDock>();

        services
            .AddOptions<GeneralOptions>()
            .Configure(options => configuration.GetSection(GeneralOptions.General).Bind(options));

        services.AddViewModelsFromAssembly(typeof(ViewModelBase));
        services.AddViewModelsFromAssembly(typeof(Program));

        // Add dockables
        var dockables = typeof(Program).Assembly.ExportedTypes
            .Where(x => typeof(IDockable).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false });

        foreach (var type in dockables)
        {
            services.AddTransient(type);
        }

        return services.BuildServiceProvider();
    }

    private static IConfiguration CreateConfiguration()
        => new ConfigurationBuilder()
            .AddJsonFile("defaultAppSettings.json", optional: false, reloadOnChange: true)
            .Build();

    private static void SetupSerilog()
    {
        const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient.IOpenAIService", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.With(new PropertyEnricher("ApplicationName", "TagToolApp"))
            .WriteTo.Debug(outputTemplate: outputTemplate, formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(
                @"C:\Users\tczyz\Documents\TagToolApp\TagToolAppLogs.txt",
                outputTemplate: outputTemplate,
                formatProvider: CultureInfo.InvariantCulture,
                shared: true)
            .CreateLogger();
    }
}
