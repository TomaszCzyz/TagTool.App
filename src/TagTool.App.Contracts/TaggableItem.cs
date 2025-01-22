namespace TagTool.App.Contracts;

public abstract class TaggableItem
{
    public Guid Id { get; set; }

    public ISet<Tag>? Tags { get; set; }
}
