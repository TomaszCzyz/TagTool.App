using TagTool.App.Models;
using TagTool.App.Views;

namespace TagTool.App.Extensions.Mappers;

public static class TriggerTypeExtensions
{
    public static TriggerType MapFromDto(this string triggerType)
        => triggerType switch
        {
            "Event" => TriggerType.Event,
            "Cron" => TriggerType.Cron,
            "Background" => TriggerType.Background,
            _ => throw new ArgumentOutOfRangeException(nameof(triggerType), triggerType, null)
        };
}
