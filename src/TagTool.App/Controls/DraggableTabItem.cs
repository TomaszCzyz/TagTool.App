using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

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
            _closeButton.Click += CloseButton_Click;
        }
        else
        {
            _closeButton.IsVisible = false;
        }
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
        Close();
    }
}
