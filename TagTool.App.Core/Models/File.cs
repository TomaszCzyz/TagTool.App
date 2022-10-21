namespace TagTool.App.Core.Models;

public record SimpleFile(int Id, string Name, long Length, DateTime? DateCreated, DateTime? DateModified, string? Location);
