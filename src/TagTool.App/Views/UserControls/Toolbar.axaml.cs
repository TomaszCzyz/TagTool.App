using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Core.Models;
using TagTool.App.ViewModels.UserControls;
using TagTool.App.Views.Dialogs;

namespace TagTool.App.Views.UserControls;

public partial class Toolbar : UserControl
{
    private readonly ToolbarViewModel _vm = App.Current.Services.GetRequiredService<ToolbarViewModel>();

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
        var dialog = new TagFileDialog();
        var showDialog2 = await dialog.ShowDialog<(string FileName, Tag[] Tags)>(GetWindow());
    }

    private async void AdvancedTaggingFileButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new AdvancedTaggingDialog();
        await dialog.ShowDialog(GetWindow());
    }
}
