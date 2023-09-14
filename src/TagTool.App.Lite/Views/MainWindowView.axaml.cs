using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using TagTool.App.Core.ViewModels;

namespace TagTool.App.Lite.Views;

public partial class MainWindowView : Window
{
    private bool _mouseDownForWindowMoving;
    private PointerPoint _originalPoint;

    public MainWindowView()
    {
        InitializeComponent();
        ClientSize = new Size(850, 600);

        AddHandler(KeyDownEvent, Window_OnKeyDown, handledEventsToo: true);

        // Focus switching between SearchBar and SearchResults
        SearchBarView.AddHandler(KeyDownEvent, SwitchFocusToSearchResults, handledEventsToo: true);
        TaggableItemsListBox.AddHandler(KeyDownEvent, SwitchFocusToSearchBar);

        // Focus switching between SearchResults and OtherResults
        TaggableItemsListBox.AddHandler(KeyDownEvent, SwitchFocusToOtherResults);
        OtherResultsListBox.AddHandler(KeyDownEvent, SwitchFocusFromOtherResultsToSearchResults);

        TaggableItemsListBox.AddHandler(KeyDownEvent, OnKeyDown_ExecuteLinkedAction, handledEventsToo: true);
        TaggableItemsListBox.AddHandler(DoubleTappedEvent, OnDoubleTapped_ExecuteLinkedAction, handledEventsToo: true);
        OtherResultsListBox.AddHandler(KeyDownEvent, OnKeyDown_ExecuteLinkedAction, handledEventsToo: true);
        OtherResultsListBox.AddHandler(DoubleTappedEvent, OnDoubleTapped_ExecuteLinkedAction, handledEventsToo: true);
    }

    private static void OnDoubleTapped_ExecuteLinkedAction(object? sender, TappedEventArgs args)
    {
        if (sender is ListBox { SelectedItem: TaggableItemViewModel vm })
        {
            vm.ExecuteLinkedActionCommand.Execute(null);
        }
    }

    private static void OnKeyDown_ExecuteLinkedAction(object? sender, KeyEventArgs args)
    {
        if (args.Key != Key.Enter)
        {
            return;
        }

        if (sender is ListBox { SelectedItem: TaggableItemViewModel vm })
        {
            vm.ExecuteLinkedActionCommand.Execute(null);
        }
    }

    private void SwitchFocusToSearchBar(object? sender, KeyEventArgs args)
    {
        if (args.Key == Key.Up)
        {
            SearchBarView.FindDescendantOfType<AutoCompleteBox>()?.Focus();
        }
    }

    private void SwitchFocusToOtherResults(object? sender, KeyEventArgs args)
    {
        if (args.Key == Key.Down && OtherResultsListBox.Items.Count != 0)
        {
            TaggableItemsListBox.Selection.Clear();

            OtherResultsListBox.ContainerFromIndex(0)?.Focus();
            OtherResultsListBox.Selection.Select(0);
        }
    }

    private void SwitchFocusFromOtherResultsToSearchResults(object? sender, KeyEventArgs args)
    {
        if (args.Key == Key.Up && TaggableItemsListBox.Items.Count != 0)
        {
            OtherResultsListBox.Selection.Clear();

            var lastItemIndex = TaggableItemsListBox.ItemCount - 1;
            TaggableItemsListBox.ContainerFromIndex(lastItemIndex)?.Focus();
            TaggableItemsListBox.Selection.Select(lastItemIndex);
        }
    }

    private void SwitchFocusToSearchResults(object? sender, KeyEventArgs args)
    {
        if (args.Key == Key.Down
            && TaggableItemsListBox.ItemCount != 0
            && !(SearchBarView.FindDescendantOfType<AutoCompleteBox>()?.IsDropDownOpen ?? false))
        {
            TaggableItemsListBox.ContainerFromIndex(0)?.Focus();
            TaggableItemsListBox.Selection.Select(0);
        }

        args.Handled = false;
    }

    private void Window_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e)
        {
            case { Key: Key.Escape }:
                WindowState = WindowState.Minimized;
                break;
            case { Key: Key.F4, KeyModifiers: KeyModifiers.Alt }:
                ShutdownApplication();
                break;
        }
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_mouseDownForWindowMoving)
        {
            return;
        }

        var currentPoint = e.GetCurrentPoint(this);
        Position = new PixelPoint(
            Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
            Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState is WindowState.Maximized or WindowState.FullScreen)
        {
            return;
        }

        _mouseDownForWindowMoving = true;
        _originalPoint = e.GetCurrentPoint(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e) => _mouseDownForWindowMoving = false;

    private void TagsSearchBar_OnLoaded(object? sender, RoutedEventArgs e) => (sender as Control)?.FindDescendantOfType<TextBox>()?.Focus();

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e) => ShutdownApplication();

    private static void ShutdownApplication()
    {
        if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime applicationLifetime)
        {
            applicationLifetime.Shutdown();
        }
    }
}
