using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TagTool.App.Extensions;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class SimpleTagsBar : UserControl
{
    public SimpleTagsBar()
    {
        DataContext = Application.Current?.CreateInstance<SimpleTagsBarViewModel>();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
