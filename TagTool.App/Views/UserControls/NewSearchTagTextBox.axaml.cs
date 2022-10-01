using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TagTool.App.UserControls;

public partial class NewSearchTagTextBox : UserControl
{
    public NewSearchTagTextBox()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
