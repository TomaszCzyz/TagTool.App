using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using TagTool.App.Core.ViewModels;

namespace TagTool.App.ViewModels.UserControls;

public enum TriggerType
{
    Schedule = 0,
    Event = 1
}

public partial class TaskTriggerViewModel : ViewModelBase
{
    private readonly Dictionary<string, string> _predefinedCronOptionsMap = new()
    {
        { "15 minutes", "*/15 * * * *" },
        { "30 minutes", "*/30 * * * *" },
        { "1 hour", "0 * * * *" },
        { "3 hour", "0 */3 * * *" },
        { "12 hour", "0 */12 * * *" },
        { "1 day", "0 0 * * *" },
        { "7 day", "0 0 */7 * *" },
        { "1 month", "0 0 0 * *" },
        { "never", "0 0 31 2 *" }
    };

    public static string[] PredefinedCronOptions { get; } =
    {
        "never", "15 minutes", "30 minutes", "1 hour", "3 hour", "12 hour", "1 day", "7 day", "1 month", "custom"
    };

    [ObservableProperty]
    private TriggerType? _triggerTypeSelectedItem = TriggerType.Schedule;

    [ObservableProperty]
    private string _cronPredefineOptionSelectedItem = PredefinedCronOptions.Last();

    [ObservableProperty]
    private string _eventTypeSelectedItem = EventTypes.Last();

    [ObservableProperty]
    private string? _customCronText;

    [NotNullIfNotNull(nameof(TriggerTypeSelectedItem))]
    public string? Cron
        => TriggerTypeSelectedItem == TriggerType.Schedule && CronPredefineOptionSelectedItem == "custom"
            ? CustomCronText
            : _predefinedCronOptionsMap[CronPredefineOptionSelectedItem];

    public TriggerType[] TriggerTypes { get; } = { TriggerType.Schedule, TriggerType.Event };

    public static string[] EventTypes { get; } = { "ItemTagged" };
}
