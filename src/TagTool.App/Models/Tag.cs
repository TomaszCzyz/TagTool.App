using Avalonia.Controls.Documents;

namespace TagTool.App.Models;

public record Tag(string? Name, InlineCollection? Inlines = null);
