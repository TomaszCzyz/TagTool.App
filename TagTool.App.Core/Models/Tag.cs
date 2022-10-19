namespace TagTool.App.Core.Models;

public class Tag
{
    public Tag(string? name)
    {
        Name = name;
    }

    public string? Name { get; set; }
}
