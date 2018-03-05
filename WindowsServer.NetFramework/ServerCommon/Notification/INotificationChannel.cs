using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Notification
{
    public interface INotificationChannel
    {
        void Notify(INotificationReceiver receiver, NotificationContent content);
        void Notify(IList<INotificationReceiver> receivers, NotificationContent content);
        Task NotifyAsync(INotificationReceiver receiver, NotificationContent content);
        Task NotifyAsync(IList<INotificationReceiver> receivers, NotificationContent content);
    }
}
