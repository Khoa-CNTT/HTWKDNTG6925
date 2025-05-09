using Models.EF;
using System;
using System.Net.Mail;
using System.Web.Helpers;
using System.Web.Mvc;
using WebsiteNoiThat.Models;
using System.Configuration;

namespace WebsiteNoiThat.Controllers
{
    public class ContactController : Controller
    {
        DBNoiThat db = new DBNoiThat();

        [HttpGet]
        public ActionResult Contacts()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contacts(EmailModel obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Lấy email đích từ Web.config
                    string toEmail = ConfigurationManager.AppSettings["FromEmailAddress"];

                    WebMail.SmtpServer = ConfigurationManager.AppSettings["SMTPHost"];
                    WebMail.SmtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                    WebMail.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnabledSSL"]);
                    WebMail.UserName = ConfigurationManager.AppSettings["FromEmailAddress"];
                    WebMail.Password = ConfigurationManager.AppSettings["FromEmailPassword"];
                    WebMail.From = WebMail.UserName;

                    // Gửi email
                    WebMail.Send(
                        to: toEmail,
                        subject: obj.EmailSubject,
                        body: "Email người gửi: " + obj.SenderEmail + "<br/><br/>" + obj.EMailBody,
                        isBodyHtml: true
                    );

                    // Lưu vào CSDL
                    var model = new Contact
                    {
                        SenderEmail = obj.SenderEmail,
                        EmailSubject = obj.EmailSubject,
                        Content = obj.EMailBody,
                        Status = true,
                        CreatedAt = DateTime.Now
                    };

                    db.Contacts.Add(model);
                    db.SaveChanges();

                    ViewBag.Status = "Cảm ơn bạn đã liên hệ. Email đã được gửi thành công!";
                    ModelState.Clear();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Status = "Có lỗi xảy ra khi gửi email: " + ex.Message;
            }

            return View();
        }
    }
}
