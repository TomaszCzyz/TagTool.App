namespace TagTool.App.Models;

public record File(int Id, string Name, long Length, DateTime? DateCreated, DateTime? DateModified, string? Location);
