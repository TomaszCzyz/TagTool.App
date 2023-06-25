namespace TagTool.App.Core.Models;

public abstract class TaggableItem
{
    public abstract string DisplayName { get; }

    public ISet<ITag>? Tags { get; set; }
}

public class TaggableFile : TaggableItem
{
    public override string DisplayName => System.IO.Path.GetFileNameWithoutExtension(Path);

    public required string Path { get; init; }
}

public class TaggableFolder : TaggableItem
{
    public override string DisplayName => System.IO.Path.GetDirectoryName(Path)!;

    public required string Path { get; init; }
}
