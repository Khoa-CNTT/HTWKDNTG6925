using System.Data.Entity.Validation;
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
    if (session == null) return RedirectToAction("Login", "Account");

    ViewBag.username = session.Username;
    LoadParIdDropDown();
    return View();
}

[HttpPost]
[HasCredential(RoleId = "ADD_CATE")]
[ValidateAntiForgeryToken]
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

    // Nếu không chọn danh mục cha → ParId là 0
    if (!n.ParId.Equals(0) && !db.Categories.Any(c => c.CategoryId == n.ParId))
    {
        ModelState.AddModelError("ParId", "Danh mục cha không tồn tại.");
    }

    if (!ModelState.IsValid)
    {
        LoadParIdDropDown();
        return View(n);
    }

    db.Categories.Add(n);
    db.SaveChanges();
    return RedirectToAction("Show");
}

private void LoadParIdDropDown()
{
    var categories = db.Categories.Select(c => new
    {
        c.CategoryId,
        c.Name
    }).ToList();

    var list = categories
        .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
        .ToList();

    // Thêm tùy chọn mặc định đầu tiên
    list.Insert(0, new SelectListItem { Value = "0", Text = "Không có danh mục cha" });

    ViewBag.ParId = new SelectList(list, "Value", "Text");
}


        [HttpGet]
        [HasCredential(RoleId = "EDIT_CATE")]
        public ActionResult Edit(int CategoryId)
        {
            var session = (UserLogin)Session[Commoncontent.user_sesion_admin];
            if (session == null) return RedirectToAction("Login", "Account");
            ViewBag.username = session.Username;

            // Lấy thông tin danh mục cần sửa
            Category a = db.Categories.SingleOrDefault(n => n.CategoryId == CategoryId);
            if (a == null) return HttpNotFound();

            LoadParIdDropDown(); // Tải dropdown với giá trị mặc định ParId = 0

            return View(a);
        }

        [HttpPost]
        [HasCredential(RoleId = "EDIT_CATE")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category n)
        {
            // Nếu không chọn danh mục cha (giá trị từ dropdown là "0"), gán ParId = 0
            if (n.ParId == null || n.ParId < 0)
            {
                n.ParId = 0;
            }

            if (!ModelState.IsValid)
            {
                LoadParIdDropDown();
                return View(n);
            }

            // Cập nhật danh mục
            db.Entry(n).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Show");
        }


        [HttpPost]
        [HasCredential(RoleId = "DELETE_CATE")]
        public ActionResult Delete(int CategoryId)
        {
            var category = db.Categories.Find(CategoryId);
            if (category != null)
            {
                // Kiểm tra xem có sản phẩm nào liên kết không
                bool hasLinkedProducts = db.Products.Any(p => p.CateId == CategoryId);
                if (hasLinkedProducts)
                {
                    TempData["DeleteError"] = "Còn sản phẩm đang được liên kết với danh mục hiện tại";
                    return RedirectToAction("Show");
                }

                db.Categories.Remove(category);
                db.SaveChanges();
            }

            return RedirectToAction("Show");
        }

    }
}
