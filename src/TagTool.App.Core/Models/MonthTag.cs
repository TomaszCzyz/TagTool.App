using System.Globalization;

namespace TagTool.App.Core.Models;

public sealed class MonthTag : ITag
{
    public string DisplayText => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month)}";

    public required int Month { get; init; }

    private bool Equals(MonthTag other) => Month == other.Month;

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is MonthTag other && Equals(other);

    public override int GetHashCode() => Month;
}
