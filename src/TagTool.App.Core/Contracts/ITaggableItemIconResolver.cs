using Avalonia.Media.Imaging;
using TagTool.App.Core.Models;

namespace TagTool.App.Core.Contracts;

public interface ITaggableItemIconResolver<in T> where T : TaggableItem
{
    Bitmap GetIcon(T item);
}
