using TagTool.BackendNew.Common;

namespace TagTool.App.Core.Extensions;

public static class TagExtensions
{
    public static Tag MapToDto(this Contracts.Tag tag) => new() { Id = tag.Id, Text = tag.Text };

    public static Contracts.Tag MapFromDto(this Tag tag) => new() { Id = tag.Id, Text = tag.Text };

    public static IEnumerable<Tag> MapToDto(this IEnumerable<Contracts.Tag> tags) => tags.Select(t => new Tag { Id = t.Id, Text = t.Text });

    public static IEnumerable<Contracts.Tag> MapFromDto(this IEnumerable<Tag> tags)
        => tags.Select(t => new Contracts.Tag { Id = t.Id, Text = t.Text });
}
