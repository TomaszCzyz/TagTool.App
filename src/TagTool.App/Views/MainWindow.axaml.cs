using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using TagTool.App.Core;
using TagTool.App.Docks;
using TagTool.App.ViewModels;

namespace TagTool.App.Views;

public partial class MainWindow : Window
{
    private double _appContentFontSizeCache = (double)AppTemplate.Current.Resources["AppContentFontSize"]!;
    private bool _mouseDownForWindowMoving;
    private PointerPoint _originalPoint;
    private IInputElement? _focusedBeforeSwitcherPopup;
    private (IDock, Document)[]? _visibleDocumentsWithOwners;

    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        AddHandler(KeyDownEvent, EscapePressedHandler);
        SwitcherPopup.AddHandler(KeyDownEvent, SwitcherPopup_OnKeyDown, RoutingStrategies.Tunnel);
        return;

        void EscapePressedHandler(object? sender, KeyEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                Focus();
            }
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        ViewModel.SaveLayoutCommand.Execute(null);
        base.OnClosing(e);
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
        var control = (Control)sender!;
        var dockControl = control.FindLogicalAncestorOfType<DocumentDockControl>();
        var myDocumentDock = (MyDocumentDock)dockControl!.DataContext!;
        var toolName = control.DataContext;
        myDocumentDock.CreateNewDocumentCommand.Execute(toolName);

        var popup = control.FindLogicalAncestorOfType<Popup>()!;
        popup.IsOpen = false;
    }

    private void ChangeFontSize(bool zoomIn)
    {
        _appContentFontSizeCache += zoomIn ? 1 : -1;
        AppTemplate.Current.Resources["AppContentFontSize"] = _appContentFontSizeCache;
    }

    private void MainWindow_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!e.KeyModifiers.Equals(KeyModifiers.Control))
        {
            return;
        }

        ChangeFontSize(e.Delta.Y < 0);

        e.Handled = true;
    }

    private void MainWindow_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e)
        {
            case { Key: Key.Add or Key.Subtract, KeyModifiers: KeyModifiers.Control }:
                ChangeFontSize(e.Key == Key.Add);

                e.Handled = true;
                break;
            case { Key: Key.Tab, KeyModifiers: KeyModifiers.Control }:
                var dockControl = this.FindLogicalDescendantOfType<DockControl>()!;
                _visibleDocumentsWithOwners = GetVisibleDocumentsWithOwners(dockControl.Layout!).ToArray();
                SwitcherListBox.ItemsSource = _visibleDocumentsWithOwners.Select(tuple => tuple.Item2.Title);

                _focusedBeforeSwitcherPopup = FocusManager?.GetFocusedElement();
                SwitcherPopup.IsOpen = true;

                if (SwitcherListBox.ItemCount != 0)
                {
                    SwitcherListBox.Selection.Select(0);
                    SwitcherListBox.ContainerFromIndex(0)?.Focus(NavigationMethod.Tab);
                }

                e.Handled = true;
                break;
        }
    }

    private void MainWindow_OnKeyUp(object? sender, KeyEventArgs e)
    {
        switch (e)
        {
            case { KeyModifiers: KeyModifiers.Control | KeyModifiers.Shift }:
                // Catch case of backwards navigation with ctrl+shift+tab to avoid popup closing. 
                break;
            case { KeyModifiers: not KeyModifiers.Control } when SwitcherPopup.IsOpen:

                if (_visibleDocumentsWithOwners is not null)
                {
                    var dockControl = this.FindLogicalDescendantOfType<DockControl>()!;
                    var (owner, doc) = _visibleDocumentsWithOwners[SwitcherListBox.SelectedIndex];
                    dockControl.Layout?.Factory?.SetActiveDockable(owner);
                    dockControl.Layout?.Factory?.SetFocusedDockable(owner, doc);

                    var view = this.GetVisualDescendants()
                        .FirstOrDefault(visual => visual is UserControl && visual.DataContext == doc);

                    (view as InputElement)?.Focus();
                }
                else
                {
                    _focusedBeforeSwitcherPopup?.Focus();
                }

                SwitcherPopup.IsOpen = false;
                e.Handled = true;
                break;
        }
    }

    private void SwitcherPopup_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var index = SwitcherListBox.SelectedIndex;
        switch (e)
        {
            case { Key: Key.Tab, KeyModifiers: KeyModifiers.Shift | KeyModifiers.Control } when SwitcherPopup.IsOpen:
                SwitcherListBox.Selection.Select((index - 1 + SwitcherListBox.ItemCount) % SwitcherListBox.ItemCount);

                e.Handled = true;
                break;
            case { Key: Key.Tab } when SwitcherPopup.IsOpen:
                SwitcherListBox.SelectedIndex = (index + 1) % SwitcherListBox.ItemCount;

                e.Handled = true;
                break;
        }
    }

    private IEnumerable<(IDock, Document)> GetVisibleDocumentsWithOwners(IDock dock)
    {
        if (dock.VisibleDockables is null)
        {
            yield break;
        }

        foreach (var dockable in dock.VisibleDockables)
        {
            if (dockable is Document document)
            {
                if (dockable.Owner is IDock iDock)
                {
                    yield return (iDock, document);
                }
            }
            else if (dockable is IDock dockInner)
            {
                foreach (var dockableInner in GetVisibleDocumentsWithOwners(dockInner))
                {
                    yield return dockableInner;
                }
            }
        }
    }
}
