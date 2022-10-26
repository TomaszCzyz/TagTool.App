using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using TagTool.App.Core.Models;
using TagTool.App.Extensions;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.Dialogs;

namespace TagTool.App.Views.UserControls;

public partial class Toolbar : UserControl
{
    private readonly ToolbarViewModel _vm = Application.Current?.CreateInstance<ToolbarViewModel>()!;

    public Toolbar()
    {
        DataContext = _vm;
        InitializeComponent();
    }

    private Window GetWindow() => VisualRoot as Window ?? throw new AggregateException("Invalid Owner");

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void AddFileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new AddFileDialog();
        var showDialog2 = await dialog.ShowDialog<(string FileName, Tag[] Tags)>(GetWindow());
    }
}
