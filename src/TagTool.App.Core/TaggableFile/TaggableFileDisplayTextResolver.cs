using JetBrains.Annotations;
using TagTool.App.Core.Contracts;

namespace TagTool.App.Core.TaggableFile;

[UsedImplicitly]
public class TaggableFileDisplayTextResolver : ITaggableItemDisplayTextResolver<TaggableFile>
{
    public string GetDisplayText(TaggableFile item) => Path.GetFileName(item.Path);
}
