using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Notification
{
    public class NotificationContent
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTime { get; set; }
        public NotificationContentSeverity Severity { get; set; }

        public NotificationContent()
        {
            CreatedTime = DateTime.UtcNow;
            Severity = NotificationContentSeverity.Normal;
        }
    }
}
