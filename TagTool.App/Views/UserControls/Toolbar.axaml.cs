using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TagTool.App.Views.UserControls;

public partial class Toolbar : UserControl
{
    public Toolbar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
