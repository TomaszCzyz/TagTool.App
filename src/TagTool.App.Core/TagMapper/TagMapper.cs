using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using TagTool.App.Core.Models;
using TagTool.Backend.DomainTypes;
using DayRangeTag = TagTool.App.Core.Models.DayRangeTag;
using DayTag = TagTool.App.Core.Models.DayTag;
using MonthRangeTag = TagTool.App.Core.Models.MonthRangeTag;
using MonthTag = TagTool.App.Core.Models.MonthTag;

namespace TagTool.App.Core.TagMapper;

/// <summary>
///     Mapper for tag types between grpc contracts and domain types.
/// </summary>
/// <remarks>
///     In the future it should be extendable to any type of tag and registration should be automated.
///     It can be achieved by registering all tag types from assemblies (mark by something or in given namespace).
///     The registration should cache all type and not using reflection, for performance reasons.
/// </remarks>
public static class TagMapper
{
    public static ITag MapToDomain(Any anyTag)
    {
        if (anyTag.Is(NormalTag.Descriptor))
        {
            var normalTag = anyTag.Unpack<NormalTag>();
            return new TextTag { Name = normalTag.Name };
        }

        if (anyTag.Is(Backend.DomainTypes.DayTag.Descriptor))
        {
            var dayTag = anyTag.Unpack<Backend.DomainTypes.DayTag>();
            return new DayTag { DayOfWeek = (DayOfWeek)dayTag.Day };
        }

        if (anyTag.Is(Backend.DomainTypes.DayRangeTag.Descriptor))
        {
            var dayRangeTag = anyTag.Unpack<Backend.DomainTypes.DayRangeTag>();
            return new DayRangeTag { Begin = (DayOfWeek)dayRangeTag.Begin, End = (DayOfWeek)dayRangeTag.End };
        }

        if (anyTag.Is(Backend.DomainTypes.MonthTag.Descriptor))
        {
            var monthTag = anyTag.Unpack<Backend.DomainTypes.MonthTag>();
            return new MonthTag { Month = monthTag.Month };
        }

        if (anyTag.Is(Backend.DomainTypes.MonthRangeTag.Descriptor))
        {
            var monthRangeTag = anyTag.Unpack<Backend.DomainTypes.MonthRangeTag>();
            return new MonthRangeTag { Begin = monthRangeTag.Begin, End = monthRangeTag.End };
        }

        if (anyTag.Is(TypeTag.Descriptor))
        {
            var typeTag = anyTag.Unpack<TypeTag>();
            return new ItemTypeTag
            {
                DisplayText = typeTag.Type switch
                {
                    "TaggableFile" => "File",
                    "TaggableFolder" => "Folder",
                    _ => throw new ArgumentException("Unable to match type of ItemTypeTag")
                }
            };
        }

        throw new ArgumentException("Unable to match tag type");
        // return  new TextTag { Name = "UnknownTagType" };
    }

    public static IMessage MapToDto(ITag tag)
        => tag switch
        {
            TextTag normalTag => new NormalTag { Name = normalTag.Name },
            DayTag dayTag => new Backend.DomainTypes.DayTag { Day = (int)dayTag.DayOfWeek },
            DayRangeTag dayRangeTag => new Backend.DomainTypes.DayRangeTag { Begin = (int)dayRangeTag.Begin, End = (int)dayRangeTag.End },
            MonthTag monthTag => new Backend.DomainTypes.MonthTag { Month = monthTag.Month },
            MonthRangeTag monthRangeTag => new Backend.DomainTypes.MonthRangeTag { Begin = monthRangeTag.Begin, End = monthRangeTag.End },
            ItemTypeTag itemTypeTag => new TypeTag
            {
                Type = itemTypeTag.DisplayText switch
                {
                    "File" => "TaggableFile",
                    "Folder" => "TaggableFolder",
                    _ => throw new ArgumentException("Unable to match type of ItemTypeTag")
                }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, null)
        };
}
