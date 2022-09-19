using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TagTool.App.ViewModels;
using TagTool.App.Views;

namespace TagTool.App;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var mainWindowViewModel = new MainWindowViewModel();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            var mainWindow = new MainWindow { DataContext = new MainWindowViewModel() };

            mainWindow.Closing += (_, _) => mainWindowViewModel.CloseLayout();

            desktopLifetime.MainWindow = mainWindow;

            desktopLifetime.Exit += (_, _) => mainWindowViewModel.CloseLayout();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
