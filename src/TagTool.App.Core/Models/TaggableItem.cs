using Avalonia.Media.Imaging;

namespace TagTool.App.Core.Models;

public abstract class TaggableItem
{
    public Guid Id { get; set; }

    public ISet<Tag>? Tags { get; set; }
}

public record TaggableItemModel(Guid Id, string DisplayName, Bitmap Icon, ISet<Tag> Tags);
