using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TagTool.App.Views;

public partial class FileView : UserControl
{
    public FileView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
