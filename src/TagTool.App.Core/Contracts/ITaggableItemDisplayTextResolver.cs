using TagTool.App.Core.Models;

namespace TagTool.App.Core.Contracts;

public interface ITaggableItemDisplayTextResolver<in T> where T : TaggableItem
{
    string GetDisplayText(T item);
}
