using Avalonia.Controls;

namespace TagTool.App.Views;

public partial class SettingsWindow : Window
{
    private SettingsWindowViewModel ViewModel => (SettingsWindowViewModel)DataContext!;

    public SettingsWindow()
    {
        InitializeComponent();
    }
}
