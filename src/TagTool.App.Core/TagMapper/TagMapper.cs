using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using TagTool.App.Core.Models;
using TagTool.Backend.DomainTypes;
using DayRangeTag = TagTool.Backend.DomainTypes.DayRangeTag;
using DayTag = TagTool.Backend.DomainTypes.DayTag;
using MonthRangeTag = TagTool.Backend.DomainTypes.MonthRangeTag;
using MonthTag = TagTool.Backend.DomainTypes.MonthTag;

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
            return  new TextTag { Name = normalTag.Name };
        }

        if (anyTag.Is(DayTag.Descriptor))
        {
            var dayTag = anyTag.Unpack<DayTag>();
            return new Models.DayTag { DayOfWeek = (DayOfWeek)dayTag.Day };
        }
        if (anyTag.Is(DayRangeTag.Descriptor))
        {
            var dayRangeTag = anyTag.Unpack<DayRangeTag>();
            return new Models.DayRangeTag { Begin = (DayOfWeek)dayRangeTag.Begin, End = (DayOfWeek)dayRangeTag.End };
        }
        if (anyTag.Is(MonthTag.Descriptor))
        {
            var monthTag = anyTag.Unpack<MonthTag>();
            return new Models.MonthTag { Month = monthTag.Month };
        }

        if (anyTag.Is(MonthRangeTag.Descriptor))
        {
            var monthRangeTag = anyTag.Unpack<MonthRangeTag>();
            return new Models.MonthRangeTag { Begin = monthRangeTag.Begin, End = monthRangeTag.End };
        }

        throw new ArgumentException("Unable to match tag type");
        // return  new TextTag { Name = "UnknownTagType" };
    }

    public static IMessage MapToDto(ITag tag)
        => tag switch
        {
            TextTag normalTag => new NormalTag { Name = normalTag.Name },
            Models.DayTag dayTag => new DayTag { Day = (int)dayTag.DayOfWeek },
            Models.DayRangeTag dayRangeTag => new DayRangeTag { Begin = (int)dayRangeTag.Begin, End = (int)dayRangeTag.End },
            Models.MonthTag monthTag => new MonthTag { Month = monthTag.Month },
            Models.MonthRangeTag monthRangeTag => new MonthRangeTag { Begin = monthRangeTag.Begin, End = monthRangeTag.End },
            _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, null)
        };
}
