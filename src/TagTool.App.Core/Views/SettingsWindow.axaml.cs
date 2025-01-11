using Avalonia.Controls;
using TagTool.App.Core.ViewModels;

namespace TagTool.App.Core.Views;

public partial class SettingsWindow : Window
{
    private SettingsWindowViewModel ViewModel => (SettingsWindowViewModel)DataContext!;

    public SettingsWindow()
    {
        InitializeComponent();
    }
}
