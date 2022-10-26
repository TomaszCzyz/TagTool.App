namespace TagTool.App.Core.Models;

public class Tag
{
    public string? Name { get; set; }

    public Tag(string? name)
    {
        Name = name;
    }

    public override string ToString() => $"{{Name: {Name}}}";
}
