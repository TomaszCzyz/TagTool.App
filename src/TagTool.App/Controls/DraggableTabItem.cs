using System.Collections;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace TagTool.App.Controls;

public partial class DraggableTabItem : TabItem
{
    private Button? _closeButton;

    public DraggableTabItem()
    {
        Closing += OnClosing;
    }

    static DraggableTabItem()
    {
        CanBeDraggedProperty.Changed.AddClassHandler<DraggableTabItem>((x, e) => x.OnCanDraggablePropertyChanged(x, e));
        IsSelectedProperty.Changed.AddClassHandler<DraggableTabItem>(UpdatePseudoClass);
        IsClosableProperty.Changed.Subscribe(e =>
        {
            if (e.Sender is DraggableTabItem { _closeButton: { } } a)
            {
                a._closeButton.IsVisible = a.IsClosable;
            }
        });
    }

    private static void UpdatePseudoClass(DraggableTabItem item, AvaloniaPropertyChangedEventArgs e)
    {
        if (!item.IsSelected)
        {
            item.PseudoClasses.Remove(":dragging");
        }
    }

    private void CloseCore()
    {
        if (Parent is not TabControl tabControl) return;

        ((IList)tabControl.Items!).Remove(this);
    }

    /// <summary>
    /// Close the Tab
    /// </summary>
    private void Close()
    {
        RaiseEvent(new RoutedEventArgs(ClosingEvent));
        CloseCore();
    }

    private void OnCanDraggablePropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        switch (CanBeDragged)
        {
            case true:
                PseudoClasses.Add(":lock-drag");
                break;
            case false:
                PseudoClasses.Remove(":lock-drag");
                break;
        }
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton")!;
        if (IsClosable)
        {
            _closeButton.Click += (_, _) =>
            {
                RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
                Close();
            };
        }
        else
        {
            _closeButton.IsVisible = false;
        }
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AddHandler(PointerPressedEvent, Pressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerMovedEvent, Moved, RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, Released, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        RemoveHandler(PointerPressedEvent, Pressed);
        RemoveHandler(PointerMovedEvent, Moved);
        RemoveHandler(PointerReleasedEvent, Released);
    }

    private bool _isDragging;
    private Point _start;
    private int _draggedIndex;
    private int _targetIndex;
    private ItemsControl? _parentItemsControl;
    private IControl? _draggedContainer;

    private static Orientation Orientation => Orientation.Horizontal;

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not DraggableTabItem { CanBeDragged: true } item) return;

        _isDragging = true;
        _start = e.GetPosition(item.Parent);
        _draggedIndex = -1;
        _targetIndex = -1;
        _parentItemsControl = item.Parent as ItemsControl; // more general
        _draggedContainer = item;

        var visual = (IVisual)sender!;
        visual.ZIndex = 12;
        // _draggedContainer.Parent.ZIndex = 100;

        AddTransforms(_parentItemsControl);
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        if (_parentItemsControl?.Items is null || _draggedContainer is null || !_isDragging) return;

        var position = e.GetPosition(_parentItemsControl);

        ((TranslateTransform)_draggedContainer.RenderTransform!).X = position.X - _start.X;
        ((TranslateTransform)_draggedContainer.RenderTransform!).Y = position.Y - _start.Y;

        var delta = Orientation == Orientation.Horizontal ? position.X - _start.X : position.Y - _start.Y;

        // if (Orientation == Orientation.Horizontal)
        // {
        //     ((TranslateTransform)_draggedContainer.RenderTransform!).X = delta;
        // }
        // else
        // {
        //     ((TranslateTransform)_draggedContainer.RenderTransform!).Y = delta;
        // }

        _draggedIndex = _parentItemsControl.ItemContainerGenerator.IndexFromContainer(_draggedContainer);
        _targetIndex = -1;

        var draggedBounds = _draggedContainer.Bounds;

        var draggedStart = Orientation == Orientation.Horizontal ? draggedBounds.X : draggedBounds.Y;

        var draggedDeltaStart = Orientation == Orientation.Horizontal ? draggedBounds.X + delta : draggedBounds.Y + delta;

        var draggedDeltaEnd = Orientation == Orientation.Horizontal
            ? draggedBounds.X + delta + draggedBounds.Width
            : draggedBounds.Y + delta + draggedBounds.Height;

        var i = 0;

        foreach (var _ in _parentItemsControl.Items)
        {
            var targetContainer = _parentItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            if (targetContainer?.RenderTransform is null || ReferenceEquals(targetContainer, _draggedContainer))
            {
                i++;
                continue;
            }

            var targetBounds = targetContainer.Bounds;

            var targetStart = Orientation == Orientation.Horizontal ? targetBounds.X : targetBounds.Y;

            var targetMid = Orientation == Orientation.Horizontal
                ? targetBounds.X + targetBounds.Width / 2
                : targetBounds.Y + targetBounds.Height / 2;

            var targetIndex = _parentItemsControl.ItemContainerGenerator.IndexFromContainer(targetContainer);

            if (targetStart > draggedStart && draggedDeltaEnd >= targetMid)
            {
                if (Orientation == Orientation.Horizontal)
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
                if (Orientation == Orientation.Horizontal)
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
            else if (Orientation == Orientation.Horizontal)
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

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging) return;

        RemoveTransforms(_parentItemsControl);

        if (_draggedIndex >= 0 && _targetIndex >= 0 && _draggedIndex != _targetIndex)
        {
            Debug.WriteLine($"MoveItem {_draggedIndex} -> {_targetIndex}");
            MoveDraggedItem(_parentItemsControl, _draggedIndex, _targetIndex);
        }

        _draggedIndex = -1;
        _targetIndex = -1;
        _isDragging = false;
        _parentItemsControl = null;
        _draggedContainer = null;
    }

    private static void AddTransforms(ItemsControl? itemsControl)
    {
        if (itemsControl?.Items is null) return;

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
        if (itemsControl?.Items is null) return;

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
        if (itemsControl?.Items is not IList items) return;

        var draggedItem = items[draggedIndex];

        items.RemoveAt(draggedIndex);
        items.Insert(targetIndex, draggedItem);

        if (itemsControl is SelectingItemsControl selectingItemsControl)
        {
            selectingItemsControl.SelectedIndex = targetIndex;
        }
    }
}
