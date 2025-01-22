using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TagTool.App.Extensions;
using TagTool.App.Views;
using MainWindowViewModel = TagTool.App.Views.MainWindowViewModel;

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
            var mainWindow = new MainWindowView { DataContext = Services.GetRequiredService<MainWindowViewModel>() };

            desktopLifetime.MainWindow = mainWindow;

            desktopLifetime.Exit += (_, _) => Log.CloseAndFlush();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        _ = ConfigureCoreServices(services);

        services.AddViewModelsFromAssembly(typeof(Program));

        return services.BuildServiceProvider();
    }
}
