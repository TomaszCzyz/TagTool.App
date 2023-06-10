using System.Globalization;
using Avalonia.Controls.Documents;

namespace TagTool.App.Models;

public record Tag(string? Name, InlineCollection? Inlines = null);

public interface ITag
{
    string DisplayText { get; }
}

public sealed record TextTag(string Name) : ITag
{
    public string DisplayText => Name;
}

public sealed record DayTag(DayOfWeek DayOfWeek) : ITag
{
    public string DisplayText => CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(DayOfWeek);
}

public sealed record DayRangeTag(DayOfWeek Begin, DayOfWeek End) : ITag
{
    public string DisplayText => $"{Begin}:{End}";
}
