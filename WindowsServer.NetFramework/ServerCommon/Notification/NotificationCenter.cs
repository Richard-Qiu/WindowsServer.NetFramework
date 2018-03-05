using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Notification
{
    public static class NotificationCenter
    {
        public static void Notify(INotificationChannel channel, INotificationReceiver receiver, NotificationContent content)
        {
            channel.Notify(receiver, content);
        }
    }
}
