using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TagTool.App.Views.UserControls;

public partial class AddTagButton : UserControl
{
    public AddTagButton()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
