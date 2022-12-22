using Avalonia.Controls.Documents;

namespace TagTool.App.Core.Models;

public record Tag(string? Name, InlineCollection? Inlines = null);
