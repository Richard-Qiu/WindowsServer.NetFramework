using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Notification.Channels
{
    public class SmtpMailNotificationReceiver : INotificationReceiver
    {
        public string ReceiverMailAddress { get; set; }
        public string ReceiverDisplayName { get; set; }
    }
}
