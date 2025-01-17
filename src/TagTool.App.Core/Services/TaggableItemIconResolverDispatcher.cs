using Avalonia.Media.Imaging;
using JetBrains.Annotations;
using TagTool.App.Core.Contracts;
using TagTool.App.Core.Models;
using TagTool.App.Core.TaggableFile;

namespace TagTool.App.Core.Services;

[UsedImplicitly]
public class TaggableItemIconResolverDispatcher : ITaggableItemIconResolver<TaggableItem>
{
    private readonly TaggableFileIconResolver _taggableFileIconResolver;

    public TaggableItemIconResolverDispatcher(TaggableFileIconResolver taggableFileIconResolver)
    {
        _taggableFileIconResolver = taggableFileIconResolver;
    }

    public Bitmap GetIcon(TaggableItem item)
    {
        return item switch
        {
            TaggableFile.TaggableFile taggableFile => _taggableFileIconResolver.GetIcon(taggableFile),
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
    }
}
