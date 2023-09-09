using System.Globalization;

namespace TagTool.App.Core.Models;

public sealed class DayTag : ITag
{
    public required DayOfWeek DayOfWeek { get; init; }

    public string DisplayText => CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(DayOfWeek);

    private bool Equals(DayTag other) => DayOfWeek == other.DayOfWeek;

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is DayTag other && Equals(other));

    public override int GetHashCode() => (int)DayOfWeek;
}
