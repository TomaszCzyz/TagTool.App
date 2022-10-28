using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TagTool.App.Core.Services;
using TagTool.App.Docks;
using TagTool.App.Options;
using TagTool.App.ViewModels;
using TagTool.App.ViewModels.Dialogs;
using TagTool.App.ViewModels.UserControls;
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
        var mainWindowViewModel = Services.GetRequiredService<MainWindowViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            var mainWindow = new MainWindow();

            mainWindow.Closing += (_, _) => mainWindowViewModel.CloseLayout();

            desktopLifetime.MainWindow = mainWindow;

            desktopLifetime.Exit += (_, _) => mainWindowViewModel.CloseLayout();
            desktopLifetime.Exit += (_, _) => Log.CloseAndFlush();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        var configuration = CreateConfiguration();

        services.AddSingleton(configuration);
        services.AddSingleton(Log.Logger);
        services.AddSingleton<IFileIconProvider, DefaultFileIconProvider>();
        services.AddSingleton<ITagToolBackendConnectionFactory, GrpcChannelFactory>();
        services.AddSingleton<ITagToolBackend, TagToolBackend>();
        services.AddTransient<NotepadFactory>();
        services.AddTransient<FilesDocumentDock>();

        services
            .AddOptions<GeneralOptions>()
            .Configure(options => configuration.GetSection(GeneralOptions.General).Bind(options));

        services
            .AddTransient<TabContentViewModel>()
            .AddTransient<ToolbarViewModel>()
            .AddTransient<TagSearchBoxViewModel>()
            .AddTransient<TagFileDialogViewModel>()
            .AddTransient<FileSystemViewModel>()
            .AddTransient<MainWindowViewModel>()
            .AddTransient<TabContentViewModel>()
            .AddTransient<SimpleTagsBarViewModel>();

        return services.BuildServiceProvider();
    }

    public static IConfiguration CreateConfiguration()
        => new ConfigurationBuilder()
            .AddJsonFile("defaultAppSettings.json", optional: false, reloadOnChange: true)
            .Build();
}
