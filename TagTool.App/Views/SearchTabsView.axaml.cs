using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TagTool.App.Views;

public partial class SearchTabsView : UserControl
{
    public SearchTabsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
