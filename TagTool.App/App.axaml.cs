using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TagTool.App.Core.Services;
using TagTool.App.ViewModels;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views;

namespace TagTool.App;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var services = ConfigureServices();
        Resources[typeof(IServiceProvider)] = services;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var mainWindowViewModel = new MainWindowViewModel();

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

        services.AddSingleton(Log.Logger);
        services.AddTransient<TabContentViewModel>();
        services.AddTransient<FileViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<TabContentViewModel>();

        return services.BuildServiceProvider();
    }
}
