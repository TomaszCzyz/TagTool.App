using Avalonia.Controls.Notifications;
using TagTool.App.Core.ViewModels;

namespace TagTool.App.ViewModels.UserControls;

public class NotificationViewModel : ViewModelBase
{
    public WindowNotificationManager? NotificationManager { get; set; }

    public string? Title { get; set; }
    public string? Message { get; set; }
}
