using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TagTool.App.Extensions;
using TagTool.App.ViewModels;

namespace TagTool.App.Views;

public partial class TabContentView : UserControl
{
    public TabContentView()
    {
        DataContext = Application.Current?.CreateInstance<TabContentViewModel>();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
