using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TagTool.App.Core.Services;
using TagTool.App.Docks;
using TagTool.App.ViewModels;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views;

namespace TagTool.App;

public class App : Application
{
    private IServiceProvider _serviceProvider = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        _serviceProvider = ConfigureServices();
        Resources[typeof(IServiceProvider)] = _serviceProvider;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            var mainWindow = new MainWindow();

            mainWindow.Closing += (_, _) => mainWindowViewModel.CloseLayout();

            desktopLifetime.MainWindow = mainWindow;

            desktopLifetime.Exit += (_, _) => mainWindowViewModel.CloseLayout();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<TagSearchServiceFactory>();
        services.AddSingleton<TagServiceFactory>();
        services.AddTransient<NotepadFactory>();
        services.AddTransient<FilesDocumentDock>();

        services.AddSingleton(Log.Logger);
        services.AddTransient<TabContentViewModel>();
        services.AddTransient<FileSystemViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<TabContentViewModel>();
        services.AddTransient<TagSearchBoxViewModel>();

        return services.BuildServiceProvider();
    }
}
