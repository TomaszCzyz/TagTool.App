namespace TagTool.App.Contracts;

public abstract class TaggableItemBase
{
    public Guid Id { get; set; }

    public ISet<Tag>? Tags { get; set; }
}
