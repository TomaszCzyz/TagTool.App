using Avalonia.Controls;

namespace TagTool.App.Core.Views;

public partial class SettingsWindow : Window
{
    private SettingsWindowViewModel ViewModel => (SettingsWindowViewModel)DataContext!;

    public SettingsWindow()
    {
        InitializeComponent();
    }
}
