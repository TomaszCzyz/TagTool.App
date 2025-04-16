namespace TagTool.App.Models;

public record InvocableDefinition
{
    public required string Id { get; init; }
    public required string GroupId { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required TriggerType TriggerType { get; init; }
    public required string Payload { get; init; }
}
