using System.Collections;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
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
        draggableTabItem.AttachedToVisualTree += Visual_OnAttachedToVisualTree;
        draggableTabItem.DetachedFromVisualTree += Visual_OnDetachedFromVisualTree;

        atw?.AddTab(draggableTabItem);
    }

    private void Visual_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var draggableTabControl = (DraggableTabItem)sender!;
        draggableTabControl.AddHandler(PointerReleasedEvent, Released, RoutingStrategies.Tunnel, handledEventsToo: true);
        draggableTabControl.AddHandler(PointerPressedEvent, Pressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        draggableTabControl.AddHandler(PointerMovedEvent, Moved, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void Visual_OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var draggableTabControl = (DraggableTabItem)sender!;
        draggableTabControl.RemoveHandler(PointerReleasedEvent, Released);
        draggableTabControl.RemoveHandler(PointerPressedEvent, Pressed);
        draggableTabControl.RemoveHandler(PointerMovedEvent, Moved);
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        var associatedObject = (InputElement)sender!;

        if (associatedObject.Parent is not ItemsControl or DraggableTabControl { ReorderableTabs: false }
            || associatedObject is DraggableTabItem { CanBeDragged: false })
        {
            return;
        }

        _enableDrag = true;
        _start = e.GetPosition((IVisual?)associatedObject.Parent);
        _draggedIndex = -1;
        _targetIndex = -1;
        _itemsControl = associatedObject.Parent as ItemsControl;
        _draggedContainer = (IControl?)associatedObject;

        AddTransforms(_itemsControl);
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (_enableDrag)
        {
            RemoveTransforms(_itemsControl);

            if (_draggedIndex >= 0 && _targetIndex >= 0 && _draggedIndex != _targetIndex)
            {
                Debug.WriteLine($"MoveItem {_draggedIndex} -> {_targetIndex}");
                MoveDraggedItem(_itemsControl, _draggedIndex, _targetIndex);
            }

            _draggedIndex = -1;
            _targetIndex = -1;
            _enableDrag = false;
            _itemsControl = null;
            _draggedContainer = null;
        }
    }

    private void AddTransforms(ItemsControl? itemsControl)
    {
        if (itemsControl?.Items is null)
        {
            return;
        }

        var i = 0;

        foreach (var _ in itemsControl.Items)
        {
            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            if (container is not null)
            {
                container.RenderTransform = new TranslateTransform();
            }

            i++;
        }
    }

    private static void RemoveTransforms(ItemsControl? itemsControl)
    {
        if (itemsControl?.Items is null)
        {
            return;
        }

        var i = 0;

        foreach (var _ in itemsControl.Items)
        {
            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            if (container is not null)
            {
                container.RenderTransform = null;
            }

            i++;
        }
    }

    private static void MoveDraggedItem(ItemsControl? itemsControl, int draggedIndex, int targetIndex)
    {
        if (itemsControl?.Items is not IList items)
        {
            return;
        }

        var draggedItem = items[draggedIndex];
        items.RemoveAt(draggedIndex);
        items.Insert(targetIndex, draggedItem);

        if (itemsControl is SelectingItemsControl selectingItemsControl)
        {
            selectingItemsControl.SelectedIndex = targetIndex;
        }
    }

    private bool _enableDrag;

    private Point _start;

    private int _draggedIndex;

    private int _targetIndex;

    private ItemsControl? _itemsControl;

    private IControl? _draggedContainer;

    public Orientation Orientation { get; } = Orientation.Horizontal;

    private void Moved(object? sender, PointerEventArgs e)
    {
        if (_itemsControl?.Items is null || _draggedContainer is null || !_enableDrag)
        {
            return;
        }

        var orientation = Orientation;
        var position = e.GetPosition(_itemsControl);
        var delta = orientation == Orientation.Horizontal ? position.X - _start.X : position.Y - _start.Y;

        if (orientation == Orientation.Horizontal)
        {
            ((TranslateTransform)_draggedContainer.RenderTransform!).X = delta;
        }
        else
        {
            ((TranslateTransform)_draggedContainer.RenderTransform!).Y = delta;
        }

        _draggedIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(_draggedContainer);
        _targetIndex = -1;

        var draggedBounds = _draggedContainer.Bounds;

        var draggedStart = orientation == Orientation.Horizontal ? draggedBounds.X : draggedBounds.Y;

        var draggedDeltaStart = orientation == Orientation.Horizontal ? draggedBounds.X + delta : draggedBounds.Y + delta;

        var draggedDeltaEnd = orientation == Orientation.Horizontal
            ? draggedBounds.X + delta + draggedBounds.Width
            : draggedBounds.Y + delta + draggedBounds.Height;

        var i = 0;

        foreach (var _ in _itemsControl.Items)
        {
            var targetContainer = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            if (targetContainer?.RenderTransform is null || ReferenceEquals(targetContainer, _draggedContainer))
            {
                i++;
                continue;
            }

            var targetBounds = targetContainer.Bounds;

            var targetStart = orientation == Orientation.Horizontal ? targetBounds.X : targetBounds.Y;

            var targetMid = orientation == Orientation.Horizontal
                ? targetBounds.X + targetBounds.Width / 2
                : targetBounds.Y + targetBounds.Height / 2;

            var targetIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(targetContainer);

            if (targetStart > draggedStart && draggedDeltaEnd >= targetMid)
            {
                if (orientation == Orientation.Horizontal)
                {
                    ((TranslateTransform)targetContainer.RenderTransform).X = -draggedBounds.Width;
                }
                else
                {
                    ((TranslateTransform)targetContainer.RenderTransform).Y = -draggedBounds.Height;
                }

                if (_targetIndex == -1)
                {
                    _targetIndex = targetIndex;
                }
                else if (targetIndex > _targetIndex)
                {
                    _targetIndex = targetIndex;
                }

                Debug.WriteLine($"Moved Right {_draggedIndex} -> {_targetIndex}");
            }
            else if (targetStart < draggedStart && draggedDeltaStart <= targetMid)
            {
                if (orientation == Orientation.Horizontal)
                {
                    ((TranslateTransform)targetContainer.RenderTransform).X = draggedBounds.Width;
                }
                else
                {
                    ((TranslateTransform)targetContainer.RenderTransform).Y = draggedBounds.Height;
                }

                if (_targetIndex == -1)
                {
                    _targetIndex = targetIndex;
                }
                else if (targetIndex < _targetIndex)
                {
                    _targetIndex = targetIndex;
                }

                Debug.WriteLine($"Moved Left {_draggedIndex} -> {_targetIndex}");
            }
            else if (orientation == Orientation.Horizontal)
            {
                ((TranslateTransform)targetContainer.RenderTransform).X = 0;
            }
            else
            {
                ((TranslateTransform)targetContainer.RenderTransform).Y = 0;
            }

            i++;
        }

        Debug.WriteLine($"Moved {_draggedIndex} -> {_targetIndex}");
    }
}
