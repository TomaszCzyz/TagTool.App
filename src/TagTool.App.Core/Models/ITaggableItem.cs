namespace TagTool.App.Core.Models;

public interface ITaggableItem
{
    public string DisplayName { get; }
}

public class TaggableFile : ITaggableItem
{
    public string DisplayName => System.IO.Path.GetFileNameWithoutExtension(Path);

    public required string Path { get; init; }
}

public class TaggableFolder : ITaggableItem
{
    public string DisplayName => System.IO.Path.GetDirectoryName(Path)!;

    public required string Path { get; init; }
}
