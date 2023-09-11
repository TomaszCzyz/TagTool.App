using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TagTool.App.Models;

public class NewNotificationMessage : ValueChangedMessage<Notification>
{
    public NewNotificationMessage(Notification notification) : base(notification)
    {
    }
}

public static class NotificationExtensions
{
    public static NewNotificationMessage ToMessage(this Notification notification)
    {
        return new NewNotificationMessage(notification);
    }
}
