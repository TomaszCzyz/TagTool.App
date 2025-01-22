using System.Text.Json;
using TagTool.App.Contracts;
using TagTool.App.Extensions;
using Tag = TagTool.BackendNew.Common.Tag;

namespace TagTool.App.Services;

public class TaggableItemMapper
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public TaggableItemBase MapToObj(string type, string taggableItem, IEnumerable<Tag> tags)
    {
        var item = type switch
        {
            "file" => JsonSerializer.Deserialize<TaggableFile.TaggableFile>(taggableItem, _jsonSerializerOptions)!,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        item.Tags = tags.MapFromDto().ToHashSet();

        return item;
    }
}
