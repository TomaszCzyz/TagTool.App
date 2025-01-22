using Avalonia.Media.Imaging;

namespace TagTool.App.Contracts;

public interface ITaggableItemIconResolver<in T> where T : TaggableItem
{
    Bitmap GetIcon(T item, int? length);
}
