using TagTool.App.Contracts;

namespace TagTool.TaggableFile;

public class TaggableFile : TaggableItem
{
    public required string Path { get; set; }
}
