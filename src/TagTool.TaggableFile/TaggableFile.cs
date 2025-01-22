using TagTool.App.Contracts;

namespace TagTool.TaggableFile;

public class TaggableFile : TaggableItemBase
{
    public required string Path { get; set; }
}
