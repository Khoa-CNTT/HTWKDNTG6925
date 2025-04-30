using Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Common;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class ProviderController : HomeController
    {
        // GET: Admin/Provider
        DBNoiThat db = new DBNoiThat();

        [HasCredential(RoleId = "VIEW_PROVIDER")]
        public ActionResult Index()
        {
            return View();
        }

        [HasCredential(RoleId = "VIEW_PROVIDER")]
        public ActionResult Show()
        {
            return View(db.Providers.ToList());
        }

        [HttpGet]
        [HasCredential(RoleId = "ADD_PROVIDER")]
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        [HasCredential(RoleId = "ADD_PROVIDER")]
        public ActionResult Add(Provider n)
        {
            if (ModelState.IsValid)
            {
                var model = db.Providers.SingleOrDefault(a => a.ProviderId == n.ProviderId);
                if (model != null)
                {
                    ModelState.AddModelError("ProError", "Id đã được sử dụng");
                    return View(n);
                }
                else
                {
                    db.Providers.Add(n);
                    db.SaveChanges();
                    return RedirectToAction("Show");
                }
            }
            return View(n);
        }

        [HttpGet]
        [HasCredential(RoleId = "EDIT_PROVIDER")]
        public ActionResult Edit(int ProviderId)
        {
            Provider a = db.Providers.SingleOrDefault(n => n.ProviderId == ProviderId);
            if (a == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhà cung cấp!";
                return RedirectToAction("Show");
            }
            return View(a);
        }

        [HttpPost]
        [HasCredential(RoleId = "EDIT_PROVIDER")]
        public ActionResult Edit(Provider n)
        {
            if (ModelState.IsValid)
            {
                db.Entry(n).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật nhà cung cấp thành công!";
                return RedirectToAction("Show");
            }
            return View(n); // Trả về model để hiển thị thông báo lỗi
        }


        [HttpGet]
        [HasCredential(RoleId = "DELETE_PROVIDER")]
        public ActionResult Delete(int id)
        {
            // Tìm nhà cung cấp theo id
            var model = db.Providers.Find(Convert.ToInt32(id));

            // Kiểm tra xem có sản phẩm nào đang sử dụng nhà cung cấp này
            var productCount = db.Products.Count(p => p.ProviderId == id);

            if (productCount > 0)
            {
                // Nếu có sản phẩm sử dụng nhà cung cấp này, hiển thị thông báo lỗi
                TempData["ErrorMessage"] = "Không thể xóa nhà cung cấp vì còn sản phẩm đang sử dụng nhà cung cấp này!";
            }
            else
            {
                // Nếu không có sản phẩm nào liên kết, tiến hành xóa nhà cung cấp
                db.Providers.Remove(model);
                db.SaveChanges();

                // Thông báo thành công
                TempData["SuccessMessage"] = "Xóa nhà cung cấp thành công!";
            }

            // Điều hướng lại trang "Show"
            return RedirectToAction("Show");
        }


    }
}