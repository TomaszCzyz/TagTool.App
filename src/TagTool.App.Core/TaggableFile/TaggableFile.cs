using TagTool.App.Core.Models;

namespace TagTool.App.Core.TaggableFile;

public class TaggableFile : TaggableItem
{
    public override string DisplayName => System.IO.Path.GetFileName(Path);

    public required string Path { get; set; }
}
