namespace TagTool.App.Models;

public interface IPayloadProperty
{
    public string Name { get; }
    public object? Value { get; }
}

// This needs to be viewmodel
public record StringProperty(object? Value, string Name, bool IsRequired) : IPayloadProperty;

public record TagProperty(object? Value, string Name, bool IsRequired) : IPayloadProperty;

public record DirectoryPathProperty(object? Value, string Name, bool IsRequired) : IPayloadProperty;
