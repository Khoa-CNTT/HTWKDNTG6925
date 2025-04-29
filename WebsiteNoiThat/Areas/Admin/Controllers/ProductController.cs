using Models.DAO;
using Models.EF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class ProductController : HomeController
    {
        DBNoiThat db = new DBNoiThat();

        [HasCredential(RoleId = "VIEW_PRODUCT")]
        public ActionResult Show()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            var productViewModels = (from a in db.Products
                                     join b in db.Providers on a.ProviderId equals b.ProviderId
                                     join c in db.Categories on a.CateId equals c.CategoryId
                                     select new ProductViewModel
                                     {
                                         ProductId = a.ProductId,
                                         Name = a.Name,
                                         Description = a.Description,
                                         Discount = a.Discount,
                                         ProviderName = b.Name,
                                         CateName = c.Name,
                                         Price = a.Price,
                                         Quantity = a.Quantity,
                                         StartDate = a.StartDate,
                                         EndDate = a.EndDate,
                                         Photo = a.Photo,
                                     }).ToList();

            return View(productViewModels);
        }

        [HttpGet]
        [HasCredential(RoleId = "ADD_PRODUCT")]
        public ActionResult Add()
        {
            ViewBag.ListCate = new SelectList(db.Categories.ToList(), "CategoryId", "Name");
            ViewBag.ListProvider = new SelectList(db.Providers.ToList(), "ProviderId", "Name");
            return View();
        }

        [HttpPost]
        [HasCredential(RoleId = "ADD_PRODUCT")]
        public ActionResult Add(ProductViewModel n, HttpPostedFileBase UploadImage)
        {
            ViewBag.ListCate = new SelectList(db.Categories.ToList(), "CategoryId", "Name");
            ViewBag.ListProvider = new SelectList(db.Providers.ToList(), "ProviderId", "Name");

            // Kiểm tra logic ngày khuyến mãi
            if (n.Discount > 0)
            {
                if (!n.StartDate.HasValue || !n.EndDate.HasValue)
                {
                    ModelState.AddModelError("", "Vui lòng nhập đầy đủ ngày bắt đầu và kết thúc khi có khuyến mãi.");
                }
                else if (n.StartDate > n.EndDate)
                {
                    ModelState.AddModelError("", "Ngày bắt đầu khuyến mãi phải nhỏ hơn ngày kết thúc.");
                }
            }
            else
            {
                if (n.StartDate.HasValue || n.EndDate.HasValue)
                {
                    ModelState.AddModelError("", "Không được nhập ngày nếu không có khuyến mãi.");
                }
            }

            if (ModelState.IsValid)
            {
                string fileName = null;
                if (UploadImage != null)
                {
                    fileName = Path.GetFileName(UploadImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/image"), fileName);
                    UploadImage.SaveAs(path);
                    n.Photo = fileName;
                }

                var product = new Product
                {
                    Name = n.Name,
                    Description = n.Description,
                    Price = n.Price,
                    Quantity = n.Quantity,
                    Photo = fileName,
                    Discount = n.Discount,
                    StartDate = n.StartDate,
                    EndDate = n.EndDate,
                    ProviderId = n.ProviderId,
                    CateId = n.CateId
                };

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Show");
            }

            return View(n);
        }




        [HttpGet]
        [HasCredential(RoleId = "EDIT_PRODUCT")]
        public ActionResult Edit(int ProductId)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            var model = (from a in db.Products
                         where a.ProductId == ProductId
                         select new ProductViewModel
                         {
                             ProductId = a.ProductId,
                             Name = a.Name,
                             Description = a.Description,
                             Discount = a.Discount,
                             Price = a.Price,
                             Quantity = a.Quantity,
                             StartDate = a.StartDate,
                             EndDate = a.EndDate,
                             Photo = a.Photo,
                             ProviderId = a.ProviderId,
                             CateId = a.CateId
                         }).FirstOrDefault();

            ViewBag.ListCate = new SelectList(db.Categories.ToList(), "CategoryId", "Name", model.CateId);
            ViewBag.ListProvider = new SelectList(db.Providers.ToList(), "ProviderId", "Name", model.ProviderId);

            return View(model);
        }

        [HttpPost]
        [HasCredential(RoleId = "EDIT_PRODUCT")]
        public ActionResult Edit(ProductViewModel n, HttpPostedFileBase UploadImage)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            ViewBag.ListCate = new SelectList(db.Categories.ToList(), "CategoryId", "Name", n.CateId);
            ViewBag.ListProvider = new SelectList(db.Providers.ToList(), "ProviderId", "Name", n.ProviderId);
            if (n.Discount > 0)
            {
                if (!n.StartDate.HasValue || !n.EndDate.HasValue)
                {
                    ModelState.AddModelError("", "Vui lòng nhập đầy đủ ngày bắt đầu và kết thúc khi có khuyến mãi.");
                }
                else if (n.StartDate > n.EndDate)
                {
                    ModelState.AddModelError("", "Ngày bắt đầu khuyến mãi phải nhỏ hơn ngày kết thúc.");
                }
            }
            else
            {
                if (n.StartDate.HasValue || n.EndDate.HasValue)
                {
                    ModelState.AddModelError("", "Không được nhập ngày nếu không có khuyến mãi.");
                }
            }
            if (ModelState.IsValid)
            {
                var model = db.Products.FirstOrDefault(m => m.ProductId == n.ProductId);
                if (model != null)
                {
                    model.Name = n.Name;
                    model.Description = n.Description;
                    model.Price = n.Price;
                    model.Quantity = n.Quantity;
                    model.Discount = n.Discount;
                    model.ProviderId = n.ProviderId;
                    model.CateId = n.CateId;
                    model.StartDate = n.StartDate;
                    model.EndDate = n.EndDate;

                    if (UploadImage != null)
                    {
                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(model.Photo))
                        {
                            var oldImagePath = Path.Combine(Server.MapPath("~/image"), model.Photo);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Lưu ảnh mới
                        string fileName = Path.GetFileName(UploadImage.FileName);
                        string path = Path.Combine(Server.MapPath("~/image"), fileName);
                        UploadImage.SaveAs(path);
                        model.Photo = fileName; // Gán tên file ảnh mới
                    }

                    db.SaveChanges();
                    return RedirectToAction("Show", new { CateId = n.CateId });
                }
            }

            return View(n); // Trả lại view với model nếu có lỗi
        }



        [HttpPost]
        [HasCredential(RoleId = "DELETE_PRODUCT")]
        public ActionResult Delete(int ProductId)
        {
            var model = db.Products.Find(ProductId); // Tìm sản phẩm theo ID
            if (model != null)
            {
                db.Products.Remove(model); // Xóa sản phẩm
                db.SaveChanges(); // Lưu thay đổi
            }
            return RedirectToAction("Show"); // Chuyển hướng về danh sách sản phẩm
        }


        public ActionResult Menu()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            var model = new CategoryDao().ListCategory();
            return PartialView(model);
        }
    }
}