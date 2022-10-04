using Avalonia;
using Avalonia.Controls;
using TagTool.App.Extensions;
using TagTool.App.ViewModels;

namespace TagTool.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
        Renderer.DrawFps = true;
#endif
        DataContext = this.CreateInstance<MainWindowViewModel>();
    }
}
