using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TagTool.App.Controls;
using TagTool.App.ViewModels;

namespace TagTool.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<MainWindowViewModel>();
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
        Renderer.DrawFps = false;
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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

    private void DraggableTabControl_OnClickOnAddingButton(object? sender, RoutedEventArgs e)
    {
        var atw = sender as DraggableTabControl;

        var draggableTabItem = new DraggableTabItem { Header = "Header", Content = new TextBlock { Text = "Content", Margin = new Thickness(10) } };
        atw?.AddTab(draggableTabItem);
    }
}
