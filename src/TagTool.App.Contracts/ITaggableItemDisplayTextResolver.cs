namespace TagTool.App.Contracts;

public interface ITaggableItemDisplayTextResolver<in T> where T : TaggableItem
{
    string GetDisplayText(T item);
}
