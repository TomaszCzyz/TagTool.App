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
        => new() { Tag = Any.Pack(TagMapper.TagMapper.MapToDto(segment.Tag)), State = MapQuerySegmentStateToDto(segment.State) };

    public static QuerySegment MapFromDto(this TagQueryParam segment)
        => new() { Tag = TagMapper.TagMapper.MapToDomain(segment.Tag), State = MapQuerySegmentStateFromDto(segment.State) };

    private static TagQueryParam.Types.QuerySegmentState MapQuerySegmentStateToDto(QuerySegmentState state)
        => state switch
        {
            QuerySegmentState.Exclude => TagQueryParam.Types.QuerySegmentState.Exclude,
            QuerySegmentState.Include => TagQueryParam.Types.QuerySegmentState.Include,
            QuerySegmentState.MustBePresent => TagQueryParam.Types.QuerySegmentState.MustBePresent,
            _ => throw new UnreachableException()
        };

    private static QuerySegmentState MapQuerySegmentStateFromDto(TagQueryParam.Types.QuerySegmentState segment)
        => segment switch
        {
            TagQueryParam.Types.QuerySegmentState.Exclude => QuerySegmentState.Exclude,
            TagQueryParam.Types.QuerySegmentState.Include => QuerySegmentState.Include,
            TagQueryParam.Types.QuerySegmentState.MustBePresent => QuerySegmentState.MustBePresent,
            _ => throw new UnreachableException()
        };
}
