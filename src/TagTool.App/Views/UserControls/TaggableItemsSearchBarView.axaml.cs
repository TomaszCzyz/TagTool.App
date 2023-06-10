using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class TaggableItemsSearchBarView : UserControl
{
    private AutoCompleteBox? _newTagControl;

    private TaggableItemsSearchBarViewModel ViewModel => (TaggableItemsSearchBarViewModel)DataContext!;

    public TaggableItemsSearchBarView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void AutoCompleteBox_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        var autoCompleteBox = (AutoCompleteBox)sender!;
        autoCompleteBox.AsyncPopulator = ViewModel.GetTagsAsync;

        _newTagControl = autoCompleteBox;
        autoCompleteBox.AddHandler(KeyDownEvent, AutoCompleteBoxOnKeyDown, RoutingStrategies.Tunnel);
    }

    private void SearchBarBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _newTagControl?.Focus();
    }

    private void AutoCompleteBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        var autoCompleteBox = (AutoCompleteBox)sender!;

        e.Handled = true;

        switch (e.Key)
        {
            case Key.Enter when autoCompleteBox.SelectedItem is not null:
                ViewModel.AddTagToSearchQueryCommand.Execute(autoCompleteBox.SelectedItem);

                // workaround for clearing Text in AutoCompleteBox when IsTextCompletionEnabled is true
                autoCompleteBox.FindDescendantOfType<TextBox>()!.Text = "";
                break;
            default:
                e.Handled = false;
                break;
        }
    }
}
