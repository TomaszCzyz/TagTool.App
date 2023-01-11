namespace TagTool.App.Models;

public interface ISpecialTag
{
    string SpecialTagName { get; }
}

public class NameSpecialTag : ISpecialTag
{
    public string SpecialTagName { get; } = nameof(NameSpecialTag);

    public string FileName { get; init; }

    public bool CaseSensitive { get; init; }

    public bool MatchSubstrings { get; init; }
}
