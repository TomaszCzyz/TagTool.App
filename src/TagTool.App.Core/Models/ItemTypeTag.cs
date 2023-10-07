namespace TagTool.App.Core.Models;

public class ItemTypeTag : ITag
{
    public required string DisplayText { get; init; }

    private bool Equals(ItemTypeTag other) => DisplayText == other.DisplayText;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((ItemTypeTag)obj);
    }

    public override int GetHashCode() => DisplayText.GetHashCode();
}
