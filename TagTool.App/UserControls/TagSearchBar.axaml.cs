using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TagTool.App.UserControls;

public partial class TagSearchBar : UserControl
{
    public TagSearchBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
