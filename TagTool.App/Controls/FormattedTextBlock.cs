using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace TagTool.App.Controls;

public class FormattedTextBlock : TextBlock
{
    public static readonly StyledProperty<StyledText?> StyledTextProperty =
        AvaloniaProperty.Register<FormattedTextBlock, StyledText>(nameof(StyledText))!;

    public static readonly StyledProperty<IReadOnlyList<FormattedTextStyleSpan>?> SpansProperty =
        AvaloniaProperty.Register<FormattedTextBlock, IReadOnlyList<FormattedTextStyleSpan>>(nameof(Spans))!;

    public FormattedTextBlock()
    {
        this.GetObservable(SpansProperty).Subscribe(_ =>
        {
            Debug.WriteLine("Unimplemented feature,FormatTextBlock.cs");
            //if(spans == null)
            //{
            //    FormattedText.Spans = null;
            //}
            //else
            //{
            //    FormattedText.Spans = spans;
            //}
        });

        this.GetObservable(StyledTextProperty).Subscribe(styledText =>
        {
            if (styledText == null)
            {
                Spans = null;
                Text = null;
            }
            else
            {
                Text = styledText.Text;
                Spans = styledText.Spans;
            }
        });
    }

    public StyledText? StyledText
    {
        get => GetValue(StyledTextProperty);
        set => SetValue(StyledTextProperty, value);
    }

    public IReadOnlyList<FormattedTextStyleSpan>? Spans
    {
        get => GetValue(SpansProperty);
        set => SetValue(SpansProperty, value);
    }
}
