namespace TagTool.App.Core.Models;

public record TaggedItem(string Name, long Length, DateTime? DateCreated, DateTime? DateModified, Tag[] Tags);
