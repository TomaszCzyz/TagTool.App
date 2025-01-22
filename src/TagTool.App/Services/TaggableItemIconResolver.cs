using Avalonia.Media.Imaging;
using JetBrains.Annotations;
using TagTool.App.Contracts;
using TagTool.TaggableFile;

namespace TagTool.App.Core.Services;

[UsedImplicitly]
public class TaggableItemIconResolver : ITaggableItemIconResolver<TaggableItemBase>
{
    private readonly TaggableFileIconResolver _taggableFileIconResolver;

    public TaggableItemIconResolver(TaggableFileIconResolver taggableFileIconResolver)
    {
        _taggableFileIconResolver = taggableFileIconResolver;
    }

    public Bitmap GetIcon(TaggableItemBase itemBase, int? length)
    {
        return itemBase switch
        {
            TaggableFile.TaggableFile taggableFile => _taggableFileIconResolver.GetIcon(taggableFile, length),
            _ => throw new ArgumentOutOfRangeException(nameof(itemBase), itemBase, null)
        };
    }
}
