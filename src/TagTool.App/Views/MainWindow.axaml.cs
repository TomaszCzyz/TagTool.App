using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Dock.Avalonia.Controls;
using TagTool.App.Docks;
using TagTool.App.ViewModels;

namespace TagTool.App.Views;

public partial class MainWindow : Window
{
    private double _appContentFontSizeCache = (double)App.Current.Resources["AppContentFontSize"]!;
    private bool _mouseDownForWindowMoving;
    private PointerPoint _originalPoint;

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        ViewModel.SaveLayoutCommand.Execute(null);
        base.OnClosing(e);
    }

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        AddHandler(KeyDownEvent, EscapePressedHandler);
        return;

        void EscapePressedHandler(object? sender, KeyEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                Focus();
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        => ViewModel.NotificationManager =
            new WindowNotificationManager((Window)VisualRoot!)
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = 3,
                Margin = new Thickness(0, 0, 15, 40)
            };

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

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var control = ((Control)sender!);
        var dockControl = control.FindLogicalAncestorOfType<DocumentDockControl>();
        var myDocumentDock = ((MyDocumentDock)dockControl!.DataContext!);
        var toolName = control.DataContext;
        myDocumentDock.CreateNewDocumentCommand.Execute(toolName);

        var popup = control.FindLogicalAncestorOfType<Popup>()!;
        popup.IsOpen = false;
    }

    private void ChangeFontSize(bool zoomIn)
    {
        _appContentFontSizeCache += zoomIn ? 1 : -1;
        App.Current.Resources["AppContentFontSize"] = _appContentFontSizeCache;
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!e.KeyModifiers.Equals(KeyModifiers.Control))
        {
            return;
        }

        ChangeFontSize(e.Delta.Y < 0);

        e.Handled = true;
    }

    private void MainContentBorder_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!e.KeyModifiers.Equals(KeyModifiers.Control) || (e.Key != Key.Add && e.Key != Key.Subtract))
        {
            return;
        }

        ChangeFontSize(e.Key == Key.Add);

        e.Handled = true;
    }
}
