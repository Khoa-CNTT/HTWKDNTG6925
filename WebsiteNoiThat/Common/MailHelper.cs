using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace WebsiteNoiThat.Common
{
    public class MailHelper
    {
        public void SendMail(string toEmailAddress, string subject, string content)
        {
            string fromEmail = ConfigurationManager.AppSettings["FromEmailAddress"];
            string fromName = ConfigurationManager.AppSettings["FromEmailDisplayName"];
            string fromPassword = ConfigurationManager.AppSettings["FromEmailPassword"];
            string smtpHost = ConfigurationManager.AppSettings["SMTPHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTPPort"]);
            bool enableSsl = bool.Parse(ConfigurationManager.AppSettings["EnabledSSL"]);

            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromEmail, fromName);
            message.To.Add(toEmailAddress);
            message.Subject = subject;
            message.Body = content;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient(smtpHost, smtpPort);
            client.EnableSsl = enableSsl;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(fromEmail, fromPassword);

            try
            {
                client.Send(message);
            }
            catch (SmtpException ex)
            {
                // Log hoặc xử lý lỗi gửi mail
                Console.WriteLine("Lỗi gửi email: " + ex.Message);
            }
        }

    }
}
