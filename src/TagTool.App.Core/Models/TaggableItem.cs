namespace TagTool.App.Core.Models;

public abstract class TaggableItem
{
    public Guid Id { get; set; }

    public ISet<Tag>? Tags { get; set; }
}
