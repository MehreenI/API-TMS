using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using API_TMS.Configuration;
using API_TMS.Models;

namespace API_TMS.Services
{
    public class MailService : IMailService
    {
        MailSettings Mail_Settings = null;
        public MailService(IOptions<MailSettings> options)
        {
            Mail_Settings = options.Value;
        }
        public async Task<bool> SendMail(Mail Mail_Data)
        {
            try
            {
                var email_Message = new MimeMessage();
                email_Message.From.Add(new MailboxAddress(Mail_Settings.Name, Mail_Settings.EmailId));
                email_Message.To.Add(new MailboxAddress(Mail_Data.EmailToName, Mail_Data.EmailToId));
                email_Message.Subject = Mail_Data.EmailSubject;

                var builder = new BodyBuilder { HtmlBody = Mail_Data.EmailBody };
                email_Message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(Mail_Settings.Host, Mail_Settings.Port, Mail_Settings.UseSSL);
                await client.AuthenticateAsync(Mail_Settings.UserName, Mail_Settings.Password);
                await client.SendAsync(email_Message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                // log ex.Message
                return false;
            }
        }
    }
}
