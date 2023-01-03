using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;

// ReSharper disable MemberCanBePrivate.Global

namespace TagTool.App.Controls;

public partial class DraggableTabItem
{
    /// <summary>
    /// Icon of the DraggableTabItem
    /// </summary>
    public IImage Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Icon"/> property.
    /// </summary>
    public static readonly StyledProperty<IImage> IconProperty
        = AvaloniaProperty.Register<DraggableTabItem, IImage>(nameof(Icon));

    /// <summary>
    /// This property sets if the DraggableTabItem can be closed
    /// </summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsClosable"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosableProperty
        = AvaloniaProperty.Register<DraggableTabItem, bool>(nameof(IsClosable), true);

    private bool _isClosing;

    /// <summary>
    /// Returns if the tab is closing.
    /// </summary>
    public bool IsClosing
    {
        get => _isClosing;
        private set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
    }

    /// <summary>
    /// Defines the <see cref="IsClosing"/> property.
    /// </summary>
    public static readonly DirectProperty<DraggableTabItem, bool> IsClosingProperty
        = AvaloniaProperty.RegisterDirect<DraggableTabItem, bool>(nameof(IsClosing), o => o.IsClosing);

    public bool CanBeDragged
    {
        get => GetValue(CanBeDraggedProperty);
        set => SetValue(CanBeDraggedProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="CanBeDragged"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanBeDraggedProperty
        = AvaloniaProperty.Register<DraggableTabItem, bool>(nameof(CanBeDragged), true);

    /// <summary>
    /// Is called before <see cref="DraggableTabItem.Closing"/> occurs
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnClosing(object? sender, RoutedEventArgs e)
    {
        IsClosing = true;
    }

    public event EventHandler<RoutedEventArgs> Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> ClosingEvent
        = RoutedEvent.Register<DraggableTabItem, RoutedEventArgs>(nameof(Closing), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> CloseButtonClick
    {
        add => AddHandler(CloseButtonClickEvent, value);
        remove => RemoveHandler(CloseButtonClickEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> CloseButtonClickEvent
        = RoutedEvent.Register<DraggableTabItem, RoutedEventArgs>(nameof(CloseButtonClick), RoutingStrategies.Bubble);
}
