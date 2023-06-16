using System;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using TagTool.App.Lite.Models;
using TagTool.Backend.DomainTypes;
using DayRangeTag = TagTool.Backend.DomainTypes.DayRangeTag;
using DayTag = TagTool.Backend.DomainTypes.DayTag;

namespace TagTool.App.Lite.TagMapper;

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
        ITag? tag = null;

        if (anyTag.Is(NormalTag.Descriptor))
        {
            var normalTag = anyTag.Unpack<NormalTag>();
            tag = new TextTag { Name = normalTag.Name };
        }
        else if (anyTag.Is(DayTag.Descriptor))
        {
            var dayTag = anyTag.Unpack<DayTag>();
            tag = new Models.DayTag { DayOfWeek = (DayOfWeek)dayTag.Day };
        }
        else if (anyTag.Is(DayRangeTag.Descriptor))
        {
            var dayRangeTag = anyTag.Unpack<DayRangeTag>();
            tag = new Models.DayRangeTag { Begin = (DayOfWeek)dayRangeTag.BeginDay, End = (DayOfWeek)dayRangeTag.EndDay };
        }

        return tag ?? new TextTag{Name = "UnknownTagType"};
        // return tag ?? throw new ArgumentException("Unable to match tag type");
    }

    public static Any MapToDto(ITag tag)
    {
        IMessage tagDto = tag switch
        {
            TextTag normalTag => new NormalTag { Name = normalTag.Name },
            Models.DayTag dayTag => new DayTag { Day = (int)dayTag.DayOfWeek },
            Models.DayRangeTag dayRangeTag => new DayRangeTag { BeginDay = (int)dayRangeTag.Begin, EndDay = (int)dayRangeTag.End },
            _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, null)
        };

        return Any.Pack(tagDto);
    }
}
