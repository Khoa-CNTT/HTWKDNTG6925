using System.Linq;
using System.Web.Mvc;
using Models.DAO;
using Models.EF;
using WebsiteNoiThat.Common;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class ProductCateController : HomeController
    {
        private DBNoiThat db = new DBNoiThat();

        public ActionResult Index()
        {
            return View();
        }

        [HasCredential(RoleId = "VIEW_CATE")]
        public ActionResult Show()
        {
            var session = (UserLogin)Session[Commoncontent.user_sesion_admin];
            if (session == null) return RedirectToAction("Login", "Account"); // Redirect if session is null
            ViewBag.username = session.Username;

            return View(db.Categories.ToList());
        }

        [HttpGet]
        [HasCredential(RoleId = "ADD_CATE")]
        public ActionResult Add()
        {
            var session = (UserLogin)Session[Commoncontent.user_sesion_admin];
            if (session == null) return RedirectToAction("Login", "Account"); // Redirect if session is null
            ViewBag.username = session.Username;

            ViewBag.ParId = new SelectList(db.Categories, "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        [HasCredential(RoleId = "ADD_CATE")]
        public ActionResult Add(Category n)
        {
            // Kiểm tra CategoryId
            if (n.CategoryId == 0)
            {
                ModelState.AddModelError("CateError", "Mã không được để trống.");
            }
            else if (n.CategoryId <= 0)
            {
                ModelState.AddModelError("CateError", "Mã phải là số nguyên dương.");
            }
            else if (db.Categories.Any(a => a.CategoryId == n.CategoryId))
            {
                ModelState.AddModelError("CateError", "Mã đã tồn tại trong hệ thống.");
            }

            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                ViewBag.ParId = new SelectList(db.Categories, "CategoryId", "Name");
                return View(n); // Trả lại dữ liệu đã nhập cùng với các lỗi
            }

            // Nếu không có lỗi, thêm vào cơ sở dữ liệu
            db.Categories.Add(n);
            db.SaveChanges();
            return RedirectToAction("Show");
        }

        [HttpGet]
        [HasCredential(RoleId = "EDIT_CATE")]
        public ActionResult Edit(int CategoryId)
        {
            var session = (UserLogin)Session[Commoncontent.user_sesion_admin];
            if (session == null) return RedirectToAction("Login", "Account"); // Redirect if session is null
            ViewBag.username = session.Username;

            // Lấy thông tin danh mục cần sửa
            Category a = db.Categories.SingleOrDefault(n => n.CategoryId == CategoryId);
            if (a == null) return HttpNotFound(); // Return 404 if not found

            // Tạo danh sách cho ParId
            ViewBag.ParId = new SelectList(db.Categories, "CategoryId", "Name", a.ParId); // Chọn danh mục cha hiện tại

            return View(a);
        }
        [HttpPost]
        [HasCredential(RoleId = "EDIT_CATE")]
        public ActionResult Edit(Category n)
        {
            

            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                ViewBag.ParId = new SelectList(db.Categories, "CategoryId", "Name", n.ParId); // Giữ lại giá trị đã chọn
                return View(n); // Trả lại dữ liệu đã nhập cùng với các lỗi
            }

            // Nếu không có lỗi, sửa đổi thông tin danh mục trong cơ sở dữ liệu
            db.Entry(n).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Show");
        }


        [HttpPost]
        [HasCredential(RoleId = "DELETE_CATE")]
        public ActionResult Delete(int CategoryId)
        {
            var model = db.Categories.Find(CategoryId);
            if (model != null)
            {
                db.Categories.Remove(model);
                db.SaveChanges();
            }
            return RedirectToAction("Show");
        }
    }
}
