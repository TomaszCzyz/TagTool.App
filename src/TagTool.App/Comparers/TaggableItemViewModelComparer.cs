using System.Collections;
using TagTool.App.ViewModels.UserControls;

namespace TagTool.App.Comparers;

public class TaggableItemViewModelComparer : IComparer
{
    public int Compare(object? x, object? y)
    {
        if (x is not TaggableItemViewModel xEntry || y is not TaggableItemViewModel yEntry)
        {
            return Comparer.Default.Compare(x, y);
        }

        return CompareInternal(xEntry, yEntry);
    }

    private static int CompareInternal(TaggableItemViewModel x, TaggableItemViewModel y)
    {
        return x.TaggedItemType switch
        {
            TaggedItemType.Folder when y.TaggedItemType == TaggedItemType.File => 1,
            TaggedItemType.File when y.TaggedItemType == TaggedItemType.Folder => -1,
            _ => string.CompareOrdinal(x.DisplayName, y.DisplayName)
        };
    }
}
