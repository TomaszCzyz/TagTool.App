using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls.Documents;

namespace TagTool.App.Core.Models;

public record Tag(string? Name, InlineCollection? Inlines = null);

public interface ITag
{
    string DisplayText { get; }
}

[DebuggerDisplay("{DisplayText}")]
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
    public string DisplayText
        => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(Begin)}-{CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(End)}";

    public DayOfWeek Begin { get; init; }

    public DayOfWeek End { get; init; }
}

public sealed class MonthTag : ITag
{
    public string DisplayText => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month)}";

    public int Month { get; set; }
}

public sealed class MonthRangeTag : ITag
{
    public string DisplayText
        => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Begin)}-{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(End)}";

    public int Begin { get; init; }

    public int End { get; init; }
}
