using Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class CustomerController : HomeController
    {
        DBNoiThat db = new DBNoiThat();
        // GET: Admin/Customer

        [HasCredential(RoleId = "VIEW_USER")]
        public ActionResult Show()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;
            var user = (from a in db.Users
                        join b in db.Cards on a.UserId equals b.UserId
                        into g
                        from d in g.DefaultIfEmpty()
                        select new UserModelView
                        {
                            UserId = a.UserId,
                            Name = a.Name,
                            Address = a.Address,
                            Phone = a.Phone,
                            Username = a.Username,
                            Password = a.Password,
                            Email = a.Email,

                            Status = a.Status,

                            GroupId = a.GroupId,
                            NumberCard = d.NumberCard,
                            Indentification = d.Identification

                        }).ToList();

            var model = user.Where(n => n.GroupId == "USER").ToList();

            return View(model);
        }

        [HttpGet]
        [HasCredential(RoleId = "ADD_USER")]
        public ActionResult Add()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;
            //ViewBag.ListGroups = new SelectList(db.UserGroups.Where(a => a.GroupId != "USER").ToList(), "GroupId", "Name");
            return View(new UserModelView());
        }

        [HttpPost]
        public ActionResult Add(UserModelView n)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            if (ModelState.IsValid)
            {
                var model = new User
                {
                    Name = n.Name,
                    Address = n.Address,
                    Phone = n.Phone,
                    Username = n.Username,
                    Password = Encryptor.MD5Hash(n.Password),
                    Email = n.Email,
                    GroupId = "USER",
                    Status = n.Status
                };

                db.Users.Add(model);
                db.SaveChanges();

                var models = new Card
                {
                    NumberCard = 0,
                    UserNumber = 0,
                    UserId = model.UserId, // Lưu ý: n.UserId không cần thiết khi dùng UserId của đối tượng User đã lưu
                    Identification = n.Indentification
                };

                db.SaveChanges();
                return RedirectToAction("Show");
            }

            return View(n);
        }


        [HttpGet]
        [HasCredential(RoleId = "EDIT_USER")]
        public ActionResult Edit(int UserId)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            // Load danh sách nhóm người dùng (nếu cần loại bỏ USER thì mở comment bên dưới)
            // ViewBag.ListRole = new SelectList(db.UserGroups.Where(a => a.GroupId != "USER").ToList(), "GroupId", "Name");

            // Load đầy đủ danh sách nhóm
            ViewBag.ListRole = new SelectList(db.UserGroups.ToList(), "GroupId", "Name");

            var user = (from a in db.Users
                        join b in db.Cards on a.UserId equals b.UserId into g
                        from d in g.DefaultIfEmpty()
                        select new UserModelView
                        {
                            UserId = a.UserId,
                            Name = a.Name,
                            Address = a.Address,
                            Phone = a.Phone,
                            Username = a.Username,
                            Password = a.Password,
                            Email = a.Email,
                            Status = a.Status,
                            GroupId = a.GroupId,
                            NumberCard = d.NumberCard,
                            Indentification = d.Identification
                        }).ToList();

            var models = user.Where(a => a.UserId == UserId).FirstOrDefault();
            if (models == null)
            {
                Response.StatusCode = 404;
                return RedirectToAction("Show");
            }

            return View(models);
        }

        [HttpPost]
        [HasCredential(RoleId = "EDIT_USER")]
        public ActionResult Edit(UserModelView userViewModel, string NewPassword)
        {
            // Kiểm tra email đã tồn tại cho user khác
            bool emailExists = db.Users.Any(u => u.Email == userViewModel.Email && u.UserId != userViewModel.UserId);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
                ViewBag.ListRole = new SelectList(db.UserGroups.ToList(), "GroupId", "Name", userViewModel.GroupId);
                return View(userViewModel);
            }

            var userInDb = db.Users.SingleOrDefault(a => a.UserId == userViewModel.UserId);
            if (userInDb != null)
            {
                // Cập nhật thông tin cơ bản
                userInDb.Name = userViewModel.Name;
                userInDb.Address = userViewModel.Address;
                userInDb.Phone = userViewModel.Phone;
                userInDb.Username = userViewModel.Username;
                userInDb.Email = userViewModel.Email;
                userInDb.GroupId = userViewModel.GroupId;
                userInDb.Status = userViewModel.Status;

                // Nếu có nhập mật khẩu mới thì mã hóa và lưu
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    userInDb.Password = Encryptor.MD5Hash(NewPassword.Trim());
                }

                // Cập nhật thẻ nếu có
                var card = db.Cards.SingleOrDefault(a => a.UserId == userInDb.UserId);
                if (card != null)
                {
                    card.NumberCard = userViewModel.NumberCard;
                    card.Identification = userViewModel.Indentification;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công!";
                return RedirectToAction("Show");
            }

            // Trường hợp không tìm thấy người dùng
            ViewBag.ListRole = new SelectList(db.UserGroups.ToList(), "GroupId", "Name", userViewModel.GroupId);
            ModelState.AddModelError("", "Không tìm thấy tài khoản.");
            return View(userViewModel);
        }



        [HttpPost]
        [HasCredential(RoleId = "DELETE_USER")]
        public ActionResult Delete(FormCollection formCollection)
        {
            string[] ids = formCollection["UserId"].Split(new char[] { ',' });
            List<string> cannotDelete = new List<string>();

            foreach (string id in ids)
            {
                int userId = Convert.ToInt32(id);
                var model = db.Users.Find(userId);

                // Kiểm tra xem User có đơn hàng không
                bool hasOrders = db.Orders.Any(o => o.UserId == userId);

                if (hasOrders)
                {
                    cannotDelete.Add(model.Name); 
                    continue;
                }

                // Xóa cart nếu có
                var cart = db.Cards.SingleOrDefault(c => c.UserId == userId);
                if (cart != null)
                {
                    db.Cards.Remove(cart);
                }

                db.Users.Remove(model);
                db.SaveChanges();
            }

            if (cannotDelete.Count > 0)
            {
                TempData["DeleteError"] = "Không thể xóa người dùng " + string.Join(", ", cannotDelete) + " vì đang có đơn hàng. ";
            }

            return RedirectToAction("Show");
        }

    }
}