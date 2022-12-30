using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Threading;

namespace TagTool.App;

public class BindableTextBlock : TextBlock
{
    public static readonly StyledProperty<InlineCollection?> InlineListProperty
        = AvaloniaProperty.Register<BindableTextBlock, InlineCollection?>(nameof(InlineList));

    public InlineCollection? InlineList
    {
        get => GetValue(InlineListProperty);
        set => SetValue(InlineListProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var textBlock = (BindableTextBlock)change.Sender;

        if (change.NewValue is InlineCollection { Count: > 0 } inlineCollection)
        {
            textBlock.Inlines?.Clear();
            textBlock.Inlines?.AddRange(inlineCollection);
        }

        // var textBlock = (change.Sender as BindableTextBlock)!;
        // if (change.NewValue is InlineCollection list)
        // {
        //     list.CollectionChanged += textBlock.InlineCollectionChanged;
        // }
    }

    private void InlineCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newInlines = e.NewItems!.Cast<Inline>().ToList();

                Inlines?.AddRange(newInlines);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                Inlines?.Clear();
            }
        });
    }
}
