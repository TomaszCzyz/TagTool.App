using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TagTool.App.Views.UserControls;

public partial class MainPanel : UserControl
{
    public MainPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
