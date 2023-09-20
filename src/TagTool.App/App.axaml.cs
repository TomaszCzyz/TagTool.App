using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Model.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TagTool.App.Core;
using TagTool.App.Core.Extensions;
using TagTool.App.Core.Services;
using TagTool.App.Docks;
using TagTool.App.Options;
using TagTool.App.Services;
using TagTool.App.ViewModels;
using TagTool.App.Views;

namespace TagTool.App;

public class App : AppTemplate
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Services = ConfigureServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            var mainWindowViewModel = Services.GetRequiredService<MainWindowViewModel>();
            var mainWindow = new MainWindow { DataContext = mainWindowViewModel };

            desktopLifetime.MainWindow = mainWindow;

            desktopLifetime.Exit += async (_, _) => await Log.CloseAndFlushAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        var configuration = ConfigureCoreServices(services);

        services.AddSingleton<IFileIconProvider, DefaultFileIconProvider>();
        services.AddSingleton<IWordHighlighter, WordHighlighter>();
        services.AddSingleton<IWorkspaceManager, WorkspaceManager>();
        services.AddSingleton<DefaultDockFactory>();
        services.AddTransient<MyDocumentDock>();
        services
            .AddOptions<GeneralOptions>()
            .Configure(options => configuration.GetSection(GeneralOptions.General).Bind(options));

        services.AddViewModelsFromAssembly(typeof(Program));

        var dockables = typeof(Program).Assembly.ExportedTypes
            .Where(x => typeof(IDockable).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false });

        foreach (var type in dockables)
        {
            services.AddTransient(type);
        }

        return services.BuildServiceProvider();
    }
}
