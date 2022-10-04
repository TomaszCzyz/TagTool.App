using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TagTool.App.Extensions;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class TagSearchBoxView : UserControl
{
    public TagSearchBoxView()
    {
        InitializeComponent();
        DataContext = Application.Current?.CreateInstance<TagSearchBoxViewModel>();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
