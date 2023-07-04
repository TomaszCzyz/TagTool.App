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
    public MainWindowView()
    {
        InitializeComponent();
        ClientSize = new Size(850, 600);

        AddHandler(KeyDownEvent, Window_OnKeyDown, handledEventsToo: true);

        // Focus switching between SearchBar and SearchResults
        SearchBarView.AddHandler(KeyDownEvent, SwitchFocusToSearchResults, handledEventsToo: true);
        TaggableItemsListBox.AddHandler(KeyDownEvent, SwitchFocusToSearchBar);

        TaggableItemsListBox.AddHandler(KeyDownEvent, ExecuteLinkedAction, handledEventsToo: true);
    }

    private static void ExecuteLinkedAction(object? sender, KeyEventArgs args)
    {
        if (args.Key != Key.Enter) return;
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

    private bool _mouseDownForWindowMoving;
    private PointerPoint _originalPoint;

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_mouseDownForWindowMoving) return;

        var currentPoint = e.GetCurrentPoint(this);
        Position = new PixelPoint(
            Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
            Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState is WindowState.Maximized or WindowState.FullScreen) return;

        _mouseDownForWindowMoving = true;
        _originalPoint = e.GetCurrentPoint(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _mouseDownForWindowMoving = false;
    }

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
