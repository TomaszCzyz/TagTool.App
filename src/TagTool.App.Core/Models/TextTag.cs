using System.Diagnostics;

namespace TagTool.App.Core.Models;

[DebuggerDisplay("{DisplayText}")]
public sealed class TextTag : ITag
{
    public required string Name { get; init; }

    public string DisplayText => Name;

    private bool Equals(TextTag other) => Name == other.Name;

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is TextTag other && Equals(other));

    public override int GetHashCode() => Name.GetHashCode();
}
