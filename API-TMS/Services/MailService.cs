using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using API_TMS.Configuration;
using API_TMS.Models;

namespace API_TMS.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        
        public MailService(IOptions<MailSettings> options)
        {
            _mailSettings = options.Value;
        }
        
        public async Task<bool> SendMail(Mail Mail_Data)
        {
            try
            {
                var email_Message = new MimeMessage();
                email_Message.From.Add(new MailboxAddress(_mailSettings.Name, _mailSettings.EmailId));
                email_Message.To.Add(new MailboxAddress(Mail_Data.EmailToName, Mail_Data.EmailToId));
                email_Message.Subject = Mail_Data.EmailSubject;

                var builder = new BodyBuilder { HtmlBody = Mail_Data.EmailBody };
                email_Message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, _mailSettings.UseSSL);
                await client.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
                await client.SendAsync(email_Message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception)
            {
                // log ex.Message
                return false;
            }
        }
    }
}
