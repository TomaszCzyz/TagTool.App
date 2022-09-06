using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TagTool.App.ViewModels;

namespace TagTool.App.Views;

public partial class TabContentView : UserControl
{
    public TabContentView()
    {
        DataContext = new TabContentViewModel();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
