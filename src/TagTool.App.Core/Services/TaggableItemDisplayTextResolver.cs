using JetBrains.Annotations;
using TagTool.App.Core.Contracts;
using TagTool.App.Core.Models;
using TagTool.App.Core.TaggableFile;

namespace TagTool.App.Core.Services;

[UsedImplicitly]
public class TaggableItemDisplayTextResolver : ITaggableItemDisplayTextResolver<TaggableItem>
{
    private readonly TaggableFileDisplayTextResolver _fileDisplayTextResolver;

    public TaggableItemDisplayTextResolver(TaggableFileDisplayTextResolver fileDisplayTextResolver)
    {
        _fileDisplayTextResolver = fileDisplayTextResolver;
    }

    public string GetDisplayText(TaggableItem item)
    {
        return item switch
        {
            TaggableFile.TaggableFile taggableFile => _fileDisplayTextResolver.GetDisplayText(taggableFile),
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
    }
}
