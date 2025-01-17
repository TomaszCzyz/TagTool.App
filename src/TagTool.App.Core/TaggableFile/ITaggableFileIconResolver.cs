using Avalonia.Media.Imaging;
using TagTool.App.Core.Contracts;

namespace TagTool.App.Core.TaggableFile;

public class TaggableFileIconResolver : ITaggableItemIconResolver<TaggableFile>
{
    public Bitmap GetIcon(TaggableFile item) => throw new NotImplementedException();
}
