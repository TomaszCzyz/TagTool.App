using TagTool.App.Contracts;

namespace TagTool.TaggableFile;

public class TaggableFile : TaggableItemBase
{
    public static string TypeName { get; } = "TaggableFile_A8ABBA71";

    public required string Path { get; set; }
}
