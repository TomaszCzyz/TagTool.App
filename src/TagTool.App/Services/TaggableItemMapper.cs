using System.Text.Json;
using TagTool.App.Contracts;

namespace TagTool.App.Core.Services;

public class TaggableItemMapper
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public TaggableItem MapToObj(string type, string taggableItem)
    {
        return type switch
        {
            "file" => JsonSerializer.Deserialize<TaggableFile.TaggableFile>(taggableItem, _jsonSerializerOptions)!,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
