using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Views.UserControls;

public partial class MainSearchBar : UserControl
{
    public MainSearchBar()
    {
        DataContext = App.Current.Services.GetRequiredService<MainSearchBarViewModel>();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        SearchHelperPopup.IsOpen ^= true;
    }

    private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        SearchHelperPopup.IsOpen = true;
    }

    private void StyledElement_OnAttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        var textBox = (TextBox)sender!;

        textBox.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        textBox.AddHandler(PointerPressedEvent, PointerPressed, RoutingStrategies.Tunnel);
    }

    private new void PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SearchHelperPopup.IsOpen = true;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var textBox = (TextBox)sender!;
        var viewModel = (MainSearchBarViewModel)DataContext!;

        switch (e.Key)
        {
            case Key.Enter: // when autoCompleteBox.SelectedItem is not null:
                viewModel.AddTagCommand.Execute(e);
                e.Handled = true;
                break;
            case Key.Back when string.IsNullOrEmpty(textBox.Text):
                viewModel.RemoveTagCommand.Execute(e);
                break;
            case Key.Right:
                SearchResultsListBox.SelectedIndex++;
                e.Handled = true;
                break;
            case Key.Left:
                SearchResultsListBox.SelectedIndex--;
                e.Handled = true;
                break;
            default:
                viewModel.UpdateSearchCommand.Execute(e);
                break;
        }
    }
}
