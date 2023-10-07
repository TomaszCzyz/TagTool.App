using System.Diagnostics;
using Google.Protobuf.WellKnownTypes;
using TagTool.Backend;

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

public static class QuerySegmentExtensions
{
    public static TagQueryParam MapToDto(this QuerySegment segment)
        => new() { Tag = Any.Pack(TagMapper.TagMapper.MapToDto(segment.Tag)), State = MapQuerySegmentState(segment) };

    private static TagQueryParam.Types.QuerySegmentState MapQuerySegmentState(QuerySegment segment)
        => segment.State switch
        {
            QuerySegmentState.Exclude => TagQueryParam.Types.QuerySegmentState.Exclude,
            QuerySegmentState.Include => TagQueryParam.Types.QuerySegmentState.Include,
            QuerySegmentState.MustBePresent => TagQueryParam.Types.QuerySegmentState.MustBePresent,
            _ => throw new UnreachableException()
        };
}
