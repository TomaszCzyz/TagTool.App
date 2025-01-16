namespace TagTool.App.Core.Models;

public abstract class TaggableItem
{
    public Guid Id { get; set; }

    public abstract string DisplayName { get; }

    public ISet<Tag>? Tags { get; set; }
}
