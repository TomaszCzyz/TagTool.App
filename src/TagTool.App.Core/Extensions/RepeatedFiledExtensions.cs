using Google.Protobuf.WellKnownTypes;
using TagTool.App.Core.Models;

namespace TagTool.App.Core.Extensions;

public static class RepeatedFiledExtensions
{
    public static IEnumerable<ITag> MapToDomain(this IEnumerable<Any> tags) => tags.Select(TagMapper.TagMapper.MapToDomain);
}
