namespace TagTool.App.Contracts;

public interface ITaggableItemDisplayTextResolver<in T> where T : TaggableItemBase
{
    string GetDisplayText(T item);
}
