using Models.EF;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private DBNoiThat db = new DBNoiThat();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ViewBag.Message = "Email không tồn tại trong hệ thống.";
                return View(model);
            }

            string newPassword = GenerateRandomPassword(8);
            string encryptedPassword = Encryptor.MD5Hash(newPassword);

            try
            {
                // Cấu hình thông tin email từ Web.config
                WebMail.SmtpServer = ConfigurationManager.AppSettings["SMTPHost"];
                WebMail.SmtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);
                WebMail.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnabledSSL"]);
                WebMail.UserName = ConfigurationManager.AppSettings["FromEmailAddress"];
                WebMail.Password = ConfigurationManager.AppSettings["FromEmailPassword"];
                WebMail.From = WebMail.UserName;

                string body = $"Xin chào {user.Name},<br/><br/>"
                            + $"Tài khoản của bạn là: <strong>{user.Username}</strong><br/>"
                            + $"Mật khẩu mới: <strong>{newPassword}</strong><br/><br/>"
                            + $"Vui lòng đăng nhập và đổi mật khẩu sau khi truy cập hệ thống.<br/>"
                            + $"<i>Trân trọng,<br/>Website Nội Thất</i>";

                WebMail.Send(
                    to: user.Email,
                    subject: "Khôi phục mật khẩu - Nội Thất Seven",
                    body: body,
                    isBodyHtml: true
                );

                // Cập nhật mật khẩu đã mã hoá vào database
                user.Password = encryptedPassword;
                db.SaveChanges();

                ViewBag.Message = "✅ Mật khẩu mới đã được gửi tới email của bạn. Vui lòng kiểm tra hộp thư đến hoặc mục spam.";
                ModelState.Clear(); // Xoá form để tránh người dùng submit lại
            }
            catch (Exception ex)
            {
                ViewBag.Message = "❌ Gửi email thất bại: " + ex.Message;
            }

            return View();
        }

        private string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            Random rand = new Random();
            return new string(Enumerable.Repeat(validChars, length)
              .Select(s => s[rand.Next(s.Length)]).ToArray());
        }
    }
}
