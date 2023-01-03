using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace TagTool.App.Controls;

public partial class DraggableTabControl : TabControl, IHeadered
{
    private Button? _adderButton;
    private const double LastSelectIndex = 0;
    private Grid? _g;

    static DraggableTabControl()
    {
        SelectionModeProperty.OverrideDefaultValue<DraggableTabControl>(SelectionMode.Single);
    }

    private void AdderButtonClicked(object? sender, RoutedEventArgs e)
    {
        var args = new RoutedEventArgs(ClickOnAddingButtonEvent);
        RaiseEvent(args);
        args.Handled = true;
    }

    protected override IItemContainerGenerator CreateItemContainerGenerator()
        => new DraggableTabItemContainerGenerator(
            this,
            ContentControl.ContentProperty,
            ContentControl.ContentTemplateProperty,
            DraggableTabItem.IconProperty,
            DraggableTabItem.IsClosableProperty);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (SelectedItem != null) return;

        var d = ((double)ItemCount / 2);

        SelectedItem = d switch
        {
            > LastSelectIndex when ItemCount != 0 => ((IList)Items!).OfType<object>().FirstOrDefault(),
            <= LastSelectIndex when ItemCount != 0 => ((IList)Items!).OfType<object>().LastOrDefault(),
            _ => SelectedItem
        };
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _adderButton = e.NameScope.Find<Button>("PART_AdderButton")!;

        _adderButton.Click += AdderButtonClicked;

        _g = e.NameScope.Find<Grid>("PART_InternalGrid")!;

        PropertyChanged += DraggableTabControl_PropertyChanged;
    }

    private void DraggableTabControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_g == null) return;

        WidthRemainingSpace = _g.Bounds.Width;
        HeightRemainingSpace = _g.Bounds.Height;
    }

    /// <summary>
    /// Add a <see cref="DraggableTabItem"/>
    /// </summary>
    /// <param name="itemToAdd">The Item to Add</param>
    public void AddTab(DraggableTabItem itemToAdd)
    {
        ((IList)Items!).Add(itemToAdd);
        itemToAdd.IsSelected = true;
    }
}
