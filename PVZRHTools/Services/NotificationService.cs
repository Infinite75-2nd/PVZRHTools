using Ursa.Controls;

namespace PVZRHTools.Services;

public class NotificationService : INotificationService
{
    public WindowNotificationManager? NotificationManager { get; set; }
}

public interface INotificationService
{
    WindowNotificationManager? NotificationManager { get; set; }
}