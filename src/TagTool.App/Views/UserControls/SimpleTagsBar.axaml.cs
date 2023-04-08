using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class SimpleTagsBar : UserControl
{
    public SimpleTagsBar()
    {
        InitializeComponent();
    }

    private void TagSearchAutoCompleteBox_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not AutoCompleteBox { DataContext: TagSearchBoxViewModel } autoCompleteBox) return;

        autoCompleteBox.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not AutoCompleteBox { DataContext: TagSearchBoxViewModel viewModel } autoCompleteBox) return;

        switch (e.Key)
        {
            case Key.Enter when autoCompleteBox.SelectedItem is not null:
                viewModel.CommitTagCommand.Execute(e);
                e.Handled = true;
                break;
            case Key.Back when string.IsNullOrEmpty(autoCompleteBox.Text):
                viewModel.RemoveTagCommand.Execute(e);
                break;
        }
    }
}
