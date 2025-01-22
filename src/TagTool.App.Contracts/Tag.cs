namespace TagTool.App.Contracts;

public record struct Tag
{
    public required int Id { get; init; }

    public required string Text { get; init; }
}
