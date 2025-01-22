using Avalonia.Media.Imaging;

namespace TagTool.App.Contracts;

public interface ITaggableItemIconResolver<in T> where T : TaggableItemBase
{
    Bitmap GetIcon(T item, int? length);
}
