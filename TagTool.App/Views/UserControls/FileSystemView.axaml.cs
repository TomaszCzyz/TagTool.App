using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TagTool.App.Extensions;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class FileSystemView : UserControl
{
    public FileSystemView()
    {
        DataContext = Application.Current?.CreateInstance<FileSystemViewModel>();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
