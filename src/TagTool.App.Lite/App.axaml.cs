using System;
using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI.GPT3;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using Serilog;
using Serilog.Core.Enrichers;
using Serilog.Events;
using Serilog.Formatting.Compact;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Services;
using TagTool.App.Core.ViewModels;
using TagTool.App.Lite.ViewModels;
using TagTool.App.Lite.Views;

namespace TagTool.App.Lite;

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
            var mainWindow = new MainWindowView { DataContext = Services.GetRequiredService<MainWindowViewModel>() };

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

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddSerilog(Log.Logger, true);
        });

        services.AddSingleton(configuration);
        services.AddSingleton<ITagToolBackendConnectionFactory, GrpcChannelFactory>();
        services.AddSingleton<ITagToolBackend, TagToolBackend>();
        services.AddTransient<ISpeechToTagSearchService, SpeechToTagSearchService>();

        services.AddOptions<OpenAiOptions>().Configure(settings =>
        {
            settings.ApiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? "throw new InvalidOperationException()";
            settings.DefaultModelId = Models.ChatGpt3_5Turbo;
        });
        services.AddHttpClient<IOpenAIService, OpenAIService>();

        services.AddViewModelsFromAssembly(typeof(ViewModelBase));
        services.AddViewModelsFromAssembly(typeof(Program));

        return services.BuildServiceProvider();
    }

    private static IConfiguration CreateConfiguration()
        => new ConfigurationBuilder()
            .AddJsonFile("defaultAppSettings.json", true, true)
            .Build();

    private static void SetupSerilog()
    {
        const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}";
        var logsPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TagTool", "App", "Logs");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient.IOpenAIService", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.With(new PropertyEnricher("ApplicationName", "TagToolApp"))
            .WriteTo.Debug(outputTemplate: outputTemplate, formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(
                new CompactJsonFormatter(),
                $"{logsPath}/logs.json",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
