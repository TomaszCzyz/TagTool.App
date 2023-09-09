using System.Globalization;

namespace TagTool.App.Core.Models;

public sealed class DayRangeTag : ITag
{
    public DayOfWeek Begin { get; init; }

    public DayOfWeek End { get; init; }

    public string DisplayText
        => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(Begin)}-{CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(End)}";

    private bool Equals(DayRangeTag other) => Begin == other.Begin && End == other.End;

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is DayRangeTag other && Equals(other));

    public override int GetHashCode() => HashCode.Combine((int)Begin, (int)End);
}
