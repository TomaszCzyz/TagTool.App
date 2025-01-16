namespace TagTool.App.Core.Models;

public record struct Tag
{
    public required Guid Id { get; init; }

    public required string Text { get; init; }

    public void Deconstruct(out Guid id, out string text)
    {
        id = Id;
        text = Text;
    }
}

public static class TagExtensions
{
    public static BackendNew.Tag MapToDto(this Tag tag) => new() { Id = tag.Id.ToString(), Text = tag.Text };

    public static Tag MapFromDto(this BackendNew.Tag tag) => new() { Id = Guid.Parse(tag.Id), Text = tag.Text };

    public static IEnumerable<BackendNew.Tag> MapToDto(this IEnumerable<Tag> tags)
        => tags.Select(t => new BackendNew.Tag { Id = t.Id.ToString(), Text = t.Text });

    public static IEnumerable<Tag> MapFromDto(this IEnumerable<BackendNew.Tag> tags)
        => tags.Select(t => new Tag { Id = Guid.Parse(t.Id), Text = t.Text });
}
