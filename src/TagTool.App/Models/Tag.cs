using System.Globalization;
using Avalonia.Controls.Documents;

namespace TagTool.App.Models;

public record Tag(string? Name, InlineCollection? Inlines = null);

public interface ITag
{
    string DisplayText { get; }
}

public sealed class TextTag : ITag
{
    public string DisplayText => Name;

    public required string Name { get; init; }
}

public sealed class DayTag : ITag
{
    public string DisplayText => CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(DayOfWeek);

    public required DayOfWeek DayOfWeek { get; init; }
}

public sealed class DayRangeTag : ITag
{
    public string DisplayText => $"{Begin}:{End}";

    public DayOfWeek Begin { get; init; }

    public DayOfWeek End { get; init; }
}
