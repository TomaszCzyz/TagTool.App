using Avalonia.Controls.Documents;

namespace TagTool.App.Models;

public record Tag(string? Name, InlineCollection? Inlines = null);

public interface ISpecialTag
{
    string SpecialTagName { get; }
}

public class NameSpecialTag : ISpecialTag
{
    public string FileName { get; init; } = null!;

    public bool CaseSensitive { get; init; }

    public bool MatchSubstrings { get; init; }

    public string SpecialTagName { get; } = nameof(NameSpecialTag);
}
