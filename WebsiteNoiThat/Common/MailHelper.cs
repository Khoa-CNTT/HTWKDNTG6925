using System.Configuration;
using System;
using System.Net;
using System.Net.Mail;

namespace WebsiteNoiThat.Common
{
    public class MailHelper
    {
        public void SendMail(string toEmailAddress, string subject, string content)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(ConfigurationManager.AppSettings["FromEmailAddress"], ConfigurationManager.AppSettings["FromEmailDisplayName"]);
            message.To.Add(toEmailAddress);
            message.Subject = subject;
            message.Body = content;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.EnableSsl = true; // Sử dụng SSL như cấu hình Gmail
            client.UseDefaultCredentials = false;

            try
            {
                client.Send(message);
            }
            catch (SmtpException ex)
            {
                // Ghi log lỗi hoặc xử lý lỗi tại đây
                Console.WriteLine("Lỗi gửi email: " + ex.Message);
            }
        }
    }
}

