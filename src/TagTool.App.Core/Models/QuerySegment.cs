using System.Diagnostics;

namespace TagTool.App.Core.Models;

[DebuggerDisplay("State: {State}, Tag: {Tag}")]
public sealed class QuerySegment
{
    public QuerySegmentState State { get; init; } = QuerySegmentState.Include;

    public required ITag Tag { get; init; }

    private bool Equals(QuerySegment other) => Tag.Equals(other.Tag);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is QuerySegment other && Equals(other));

    public override int GetHashCode() => Tag.GetHashCode();
}
