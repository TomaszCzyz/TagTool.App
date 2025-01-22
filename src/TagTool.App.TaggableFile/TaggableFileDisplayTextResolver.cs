using JetBrains.Annotations;
using TagTool.App.Contracts;

namespace TagTool.TaggableFile;

[UsedImplicitly]
public class TaggableFileDisplayTextResolver : ITaggableItemDisplayTextResolver<TaggableFile>
{
    public string GetDisplayText(TaggableFile item) => Path.GetFileName(item.Path);
}
