using Avalonia.Media.Imaging;
using TagTool.App.Contracts;

namespace TagTool.App.Views;

public class TaggableItemViewModel
{
    public TaggableItemBase TaggableItem { get; init; }
    public Bitmap? Icon { get; init; }
    public string DisplayName { get; init; }
    public ISet<Tag> Tags => TaggableItem.Tags ?? new HashSet<Tag>();

    public TaggableItemViewModel(TaggableItemBase taggableItem, Bitmap? icon, string displayName)
    {
        TaggableItem = taggableItem;
        Icon = icon;
        DisplayName = displayName;
    }
}
