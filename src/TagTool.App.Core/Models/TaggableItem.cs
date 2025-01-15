namespace TagTool.App.Core.Models;

public abstract class TaggableItem
{
    public Guid Id { get; set; }

    public abstract string DisplayName { get; }

    public IReadOnlySet<Tag>? Tags { get; init; }
}

public class TaggableFile : TaggableItem
{
    public override string DisplayName => System.IO.Path.GetFileName(Path);

    public required string Path { get; init; }
}

public class TaggableFolder : TaggableItem
{
    public override string DisplayName => System.IO.Path.GetFileName(Path);

    public required string Path { get; init; }
}
