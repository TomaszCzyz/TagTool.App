using TagTool.App.Core.Models;

namespace TagTool.App.Core.TaggableFile;

public class TaggableFile : TaggableItem
{
    public required string Path { get; set; }
}
