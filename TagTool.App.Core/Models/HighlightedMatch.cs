using Avalonia.Controls.Documents;

namespace TagTool.App.Core.Models;

public record HighlightInfo(int StartIndex, int Length);

public class HighlightedMatch
{
    public string? MatchedText { get; set; } = "default";

    public InlineCollection? Inlines { get; set; }

    public int Score { get; init; }

    private sealed class ScoreRelationalComparer : IComparer<HighlightedMatch>
    {
        public int Compare(HighlightedMatch? x, HighlightedMatch? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return -x.Score.CompareTo(y.Score);
        }
    }

    public static IComparer<HighlightedMatch> ScoreComparer { get; } = new ScoreRelationalComparer();
}
