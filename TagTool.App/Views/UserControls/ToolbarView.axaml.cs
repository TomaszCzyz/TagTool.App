using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TagTool.App.Views;

public partial class ToolbarView : UserControl
{
    public ToolbarView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
