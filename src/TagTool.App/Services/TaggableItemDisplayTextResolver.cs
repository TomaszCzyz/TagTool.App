using JetBrains.Annotations;
using TagTool.App.Contracts;
using TagTool.TaggableFile;

namespace TagTool.App.Core.Services;

[UsedImplicitly]
public class TaggableItemDisplayTextResolver : ITaggableItemDisplayTextResolver<TaggableItemBase>
{
    private readonly TaggableFileDisplayTextResolver _fileDisplayTextResolver;

    public TaggableItemDisplayTextResolver(TaggableFileDisplayTextResolver fileDisplayTextResolver)
    {
        _fileDisplayTextResolver = fileDisplayTextResolver;
    }

    public string GetDisplayText(TaggableItemBase itemBase)
    {
        return itemBase switch
        {
            TaggableFile.TaggableFile taggableFile => _fileDisplayTextResolver.GetDisplayText(taggableFile),
            _ => throw new ArgumentOutOfRangeException(nameof(itemBase), itemBase, null)
        };
    }
}
