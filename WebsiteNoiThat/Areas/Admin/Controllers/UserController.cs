using Models.EF;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class UserController : HomeController
    {
        DBNoiThat db = new DBNoiThat();

        public ActionResult Index()
        {
            return View();
        }

        // Admin có thể xem tất cả người dùng
        [HasCredential(RoleId = "VIEW_ADMIN")]
        public ActionResult Show()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            var models = db.Users.Where(n => n.GroupId != "USER").ToList();
            return View(models);
        }

        // Admin có thể thêm người dùng
        [HttpGet]
        [HasCredential(RoleId = "ADD_ADMIN")]
        public ActionResult Add()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            ViewBag.ListGroups = new SelectList(db.UserGroups.Where(a => a.GroupId != "USER").ToList(), "GroupId", "Name");
            return View();
        }

        // Admin xử lý việc thêm người dùng
        [HttpPost]
        [HasCredential(RoleId = "ADD_ADMIN")]
        public ActionResult Add(User n)
        {
            ViewBag.ListGroups = new SelectList(db.UserGroups.Where(a => a.GroupId != "USER").ToList(), "GroupId", "Name");
            var model = new User
            {
                Name = n.Name,
                Address = n.Address,
                Phone = n.Phone,
                Username = n.Username,
                Password = Encryptor.MD5Hash(n.Password),  // Mã hóa mật khẩu
                Email = n.Email,
                GroupId = n.GroupId,
                Status = n.Status
            };
            db.Users.Add(model);
            db.SaveChanges();
            return RedirectToAction("Show");
        }

        // Admin chỉnh sửa thông tin người dùng
        [HttpGet]
        [HasCredential(RoleId = "EDIT_ADMIN")]
        public ActionResult Edit(int UserId)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            ViewBag.ListGroups = new SelectList(db.UserGroups.Where(a => a.GroupId != "USER").ToList(), "GroupId", "Name");
            var user = db.Users.FirstOrDefault(a => a.UserId == UserId);
            if (user == null)
            {
                Response.StatusCode = 404;
                return RedirectToAction("Show");
            }
            return View(user);
        }

        // Admin xử lý việc chỉnh sửa người dùng
        [HttpPost]
        [HasCredential(RoleId = "EDIT_ADMIN")]
        public ActionResult Edit(User user, string NewPassword)
        {
            var userInDb = db.Users.Find(user.UserId);
            if (userInDb != null)
            {
                userInDb.Name = user.Name;
                userInDb.Phone = user.Phone;
                userInDb.Address = user.Address;
                userInDb.Email = user.Email;
                userInDb.Username = user.Username;
                userInDb.GroupId = user.GroupId;
                userInDb.Status = user.Status;

                if (!string.IsNullOrEmpty(NewPassword))
                {
                    userInDb.Password = Encryptor.MD5Hash(NewPassword);
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công!";
                return RedirectToAction("Show");
            }

            ModelState.AddModelError("", "Không tìm thấy tài khoản.");
            return View(user);
        }

        // Admin hoặc User có thể xóa người dùng
        [HttpPost]
        [HasCredential(RoleId = "DELETE_ADMIN")]
        public ActionResult Delete(FormCollection formCollection)
        {
            string[] ids = formCollection["UserId"].Split(new char[] { ',' });

            foreach (string id in ids)
            {
                int userId = Convert.ToInt32(id);
                var model = db.Users.Find(userId);

                // Kiểm tra nếu người dùng có đơn hàng thì không cho xóa
                bool hasOrders = db.Orders.Any(o => o.UserId == userId);
                if (hasOrders)
                {
                    TempData["Error"] = $"Không thể xóa tài khoản có đơn hàng (UserId: {userId}).";
                    continue; // bỏ qua và xử lý tiếp user khác
                }

                db.Users.Remove(model);
                db.SaveChanges();
            }

            return RedirectToAction("Show");
        }


        // User có thể chỉnh sửa thông tin cá nhân của chính mình
        [HttpGet]
        public ActionResult UserProfile()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            if (session == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = db.Users.FirstOrDefault(n => n.UserId == session.UserId);
            if (model == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.username = session.Username;
            return View(model);
        }

        // User xử lý việc cập nhật thông tin cá nhân
        [HttpPost]
        public ActionResult UserProfile(User user, string NewPassword)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            if (session == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userInDb = db.Users.Find(user.UserId);
            if (userInDb != null)
            {
                userInDb.Name = user.Name;
                userInDb.Phone = user.Phone;
                userInDb.Address = user.Address;
                userInDb.Email = user.Email;
                userInDb.Status = user.Status;

                if (!string.IsNullOrEmpty(NewPassword))
                {
                    userInDb.Password = Encryptor.MD5Hash(NewPassword); // Mã hóa mật khẩu
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công!";
                return RedirectToAction("UserProfile");
            }

            ModelState.AddModelError("", "Không tìm thấy tài khoản.");
            return View(user);
        }
    }
}
