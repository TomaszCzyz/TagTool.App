using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

// ReSharper disable MemberCanBePrivate.Global

namespace TagTool.App.Controls;

public partial class DraggableTabControl
{
    private object _fallbackContent = new TextBlock
    {
        Text = "Nothing here",
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        FontSize = 16
    };

    /// <summary>
    /// This content is showed when there is no item.
    /// </summary>
    public object FallBackContent
    {
        get => _fallbackContent;
        set => SetAndRaise(FallBackContentProperty, ref _fallbackContent, value);
    }

    /// <summary>
    /// Defines the <see cref="FallBackContent"/> property.
    /// </summary>
    public static readonly DirectProperty<DraggableTabControl, object> FallBackContentProperty
        = AvaloniaProperty.RegisterDirect<DraggableTabControl, object>(
            nameof(FallBackContent),
            o => o.FallBackContent,
            (o, v) => o.FallBackContent = v);

    /// <summary>
    /// Gets or sets the Header.
    /// </summary>
    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Header"/> property.
    /// </summary>
    public static readonly StyledProperty<object> HeaderProperty =
        AvaloniaProperty.Register<DraggableTabControl, object>(nameof(Header));

    /// <summary>
    /// Gets or sets the Header Template.
    /// </summary>
    public ITemplate HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="HeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<ITemplate> HeaderTemplateProperty =
        AvaloniaProperty.Register<DraggableTabControl, ITemplate>(nameof(HeaderTemplate));

    /// <summary>
    /// This property defines if the AdderButton can be visible, the default value is true.
    /// </summary>
    public bool AdderButtonIsVisible
    {
        get => GetValue(AdderButtonIsVisibleProperty);
        set => SetValue(AdderButtonIsVisibleProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="AdderButtonIsVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> AdderButtonIsVisibleProperty
        = AvaloniaProperty.Register<DraggableTabControl, bool>(nameof(AdderButtonIsVisible), true);

    /// <summary>
    /// This property defines what is the maximum width of the ItemsPresenter.
    /// </summary>
    public double MaxWidthOfItemsPresenter
    {
        get => GetValue(MaxWidthOfItemsPresenterProperty);
        set => SetValue(MaxWidthOfItemsPresenterProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MaxWidthOfItemsPresenter"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxWidthOfItemsPresenterProperty
        = AvaloniaProperty.Register<DraggableTabControl, double>(nameof(MaxWidthOfItemsPresenter), double.PositiveInfinity);

    /// <summary>
    /// Gets or Sets the SecondaryBackground.
    /// </summary>
    public IBrush SecondaryBackground
    {
        get => GetValue(SecondaryBackgroundProperty);
        set => SetValue(SecondaryBackgroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> SecondaryBackgroundProperty
        = AvaloniaProperty.Register<DraggableTabControl, IBrush>(nameof(SecondaryBackground));

    /// <summary>
    /// Sets the margin of the itemspresenter
    /// </summary>
    public Thickness ItemsMargin
    {
        get => GetValue(ItemsMarginProperty);
        set => SetValue(ItemsMarginProperty, value);
    }

    /// <summary>
    ///
    /// </summary>
    public static readonly StyledProperty<Thickness> ItemsMarginProperty
        = AvaloniaProperty.Register<DraggableTabControl, Thickness>(nameof(ItemsMargin));

    private double _heightRemainingSpace;

    /// <summary>
    /// Gets the space that remains in the top
    /// </summary>
    public double HeightRemainingSpace
    {
        get => _heightRemainingSpace;
        private set => SetAndRaise(HeightRemainingSpaceProperty, ref _heightRemainingSpace, value);
    }

    /// <summary>
    /// Defines the <see cref="HeightRemainingSpace"/> property.
    /// </summary>
    public static readonly DirectProperty<DraggableTabControl, double> HeightRemainingSpaceProperty
        = AvaloniaProperty.RegisterDirect<DraggableTabControl, double>(nameof(HeightRemainingSpace), o => o.HeightRemainingSpace);

    private double _widthRemainingSpace;

    /// <summary>
    /// Gets the space that remains in the top.
    /// </summary>
    public double WidthRemainingSpace
    {
        get => _widthRemainingSpace;
        private set => SetAndRaise(WidthRemainingSpaceProperty, ref _widthRemainingSpace, value);
    }

    /// <summary>
    /// Defines the <see cref="WidthRemainingSpace"/> property.
    /// </summary>
    public static readonly DirectProperty<DraggableTabControl, double> WidthRemainingSpaceProperty
        = AvaloniaProperty.RegisterDirect<DraggableTabControl, double>(nameof(WidthRemainingSpace), o => o.WidthRemainingSpace);

    /// <summary>
    /// Gets or Sets if the Children-Tabs can be reorganized by dragging.
    /// </summary>
    public bool ReorderableTabs
    {
        get => GetValue(ReorderableTabsProperty);
        set => SetValue(ReorderableTabsProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ReorderableTabs"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ReorderableTabsProperty
        = AvaloniaProperty.Register<DraggableTabControl, bool>(nameof(ReorderableTabs), true);

    /// <summary>
    /// Gets or sets if the DraggableTabsChildren can be dragged Immediate or on PointerReleased only.
    /// </summary>
    public bool ImmediateDrag
    {
        get => GetValue(ImmediateDragProperty);
        set => SetValue(ImmediateDragProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ImmediateDrag"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ImmediateDragProperty
        = AvaloniaProperty.Register<DraggableTabControl, bool>(nameof(ImmediateDrag), true);

    /// <summary>
    /// It's raised when the adder button is clicked
    /// </summary>
    public event EventHandler<RoutedEventArgs> ClickOnAddingButton
    {
        add => AddHandler(ClickOnAddingButtonEvent, value);
        remove => RemoveHandler(ClickOnAddingButtonEvent, value);
    }

    /// <summary>
    /// Defines the <see cref="ClickOnAddingButton"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickOnAddingButtonEvent
        = RoutedEvent.Register<DraggableTabControl, RoutedEventArgs>(nameof(ClickOnAddingButton), RoutingStrategies.Bubble);
}
