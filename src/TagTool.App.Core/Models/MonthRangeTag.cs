using System.Globalization;

namespace TagTool.App.Core.Models;

public sealed class MonthRangeTag : ITag
{
    public string DisplayText
        => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Begin)}-{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(End)}";

    public int Begin { get; init; }

    public int End { get; init; }

    private bool Equals(MonthRangeTag other) => Begin == other.Begin && End == other.End;

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is MonthRangeTag other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Begin, End);
}
