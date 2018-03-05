using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Web;

namespace WindowsServer.Notification.Channels
{
    public class SmtpMailNotificationChannel : INotificationChannel
    {
        private string _smtpConfiguration;
        private string _senderAddress;
        private string _senderDisplayName;

        public SmtpMailNotificationChannel(string smtpConfiguration, string senderAddress, string senderDisplayName)
        {
            _smtpConfiguration = smtpConfiguration;
            _senderAddress = senderAddress;
            _senderDisplayName = senderDisplayName;
        }

        private MailMessage BuildMailMessage(SmtpMailNotificationReceiver receiver, NotificationContent content)
        {
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(_senderAddress, _senderDisplayName, Encoding.UTF8);
            mailMessage.To.Add(new MailAddress(receiver.ReceiverMailAddress, receiver.ReceiverDisplayName));
            mailMessage.Subject = content.Title;
            mailMessage.IsBodyHtml = false;
            mailMessage.Body = content.Description;
            return mailMessage;
        }

        public void Notify(INotificationReceiver receiver, NotificationContent content)
        {
            var mail = BuildMailMessage(receiver as SmtpMailNotificationReceiver, content);
            // Because SmtpClient.Send() is not thread safe, do not make it static.
            // Create smtp client object on the fly.
            var smtpClient = MailUtility.CreateSmtpClientFromConfiguration(_smtpConfiguration);
            smtpClient.Send(mail);
        }

        public void Notify(IList<INotificationReceiver> receivers, NotificationContent content)
        {
            throw new NotImplementedException();
        }

        public async Task NotifyAsync(INotificationReceiver receiver, NotificationContent content)
        {
            await Task.FromResult(0);
            throw new NotImplementedException();
        }

        public async Task NotifyAsync(IList<INotificationReceiver> receivers, NotificationContent content)
        {
            await Task.FromResult(0);
            throw new NotImplementedException();
        }
    }
}
