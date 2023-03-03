using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TagTool.App.Models.Messages;

public class NewNotificationMessage : ValueChangedMessage<Notification>
{
    public NewNotificationMessage(Notification notification) : base(notification)
    {
    }
}
