using Avalonia.Media.Imaging;
using JetBrains.Annotations;
using TagTool.App.Contracts;
using TagTool.TaggableFile;

namespace TagTool.App.Core.Services;

[UsedImplicitly]
public class TaggableItemIconResolver : ITaggableItemIconResolver<TaggableItem>
{
    private readonly TaggableFileIconResolver _taggableFileIconResolver;

    public TaggableItemIconResolver(TaggableFileIconResolver taggableFileIconResolver)
    {
        _taggableFileIconResolver = taggableFileIconResolver;
    }

    public Bitmap GetIcon(TaggableItem item, int? length)
    {
        return item switch
        {
            TaggableFile.TaggableFile taggableFile => _taggableFileIconResolver.GetIcon(taggableFile, length),
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
    }
}
