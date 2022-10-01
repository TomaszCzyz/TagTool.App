using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TagTool.App.Extensions;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

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
