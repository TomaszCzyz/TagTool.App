using Avalonia.Controls.Documents;
using Avalonia.Media;
using TagTool.App.Core.Models;

namespace TagTool.App.Core.Services;

public interface IWordHighlighter
{
    InlineCollection CreateInlines(string tagName, IReadOnlyCollection<HighlightInfo> highlightInfos);
}

public class WordHighlighter : IWordHighlighter
{
    public InlineCollection CreateInlines(string tagName, IReadOnlyCollection<HighlightInfo> highlightInfos)
    {
        var inlines = new InlineCollection();

        var lastIndex = 0;
        var index = 0;

        while (index < tagName.Length)
        {
            var highlightedPart = highlightInfos.FirstOrDefault(info => info.StartIndex == index);

            if (highlightedPart is null)
            {
                index++;
            }
            else
            {
                FlushNotHighlighted();

                var endIndex = index + highlightedPart.Length;

                var solidColorBrush = new SolidColorBrush(Color.Parse("#0F7EBD"));
                var run = new Run
                {
                    Text = tagName[index..endIndex],
                    TextDecorations = new TextDecorationCollection { new() { Location = TextDecorationLocation.Underline, Stroke = solidColorBrush } }
                    // FontWeight = FontWeight.Bold,
                    // Foreground = new SolidColorBrush(Color.Parse("#EEEEEE"));
                    // Background = solidColorBrush;
                };
                inlines.Add(run);

                index = endIndex;
                lastIndex = index;
            }
        }

        FlushNotHighlighted();

        void FlushNotHighlighted()
        {
            if (lastIndex == index) return;

            inlines.Add(new Run { Text = tagName[lastIndex..index] });
        }

        return inlines;
    }
}
