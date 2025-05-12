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
                                         IsVisible = a.IsVisible 

                                     }).ToList();

            return View(productViewModels);
        }

        private void LoadViewBags(object selectedCateId = null, object selectedProviderId = null)
        {
            ViewBag.ListCate = new SelectList(db.Categories.ToList(), "CategoryId", "Name", selectedCateId);
            ViewBag.ListProvider = new SelectList(db.Providers.ToList(), "ProviderId", "Name", selectedProviderId);
        }

        [HttpGet]
        [HasCredential(RoleId = "ADD_PRODUCT")]
        public ActionResult Add()
        {
            LoadViewBags();
            return View();
        }

        [HttpPost]
        [HasCredential(RoleId = "ADD_PRODUCT")]
        public ActionResult Add(ProductViewModel model, HttpPostedFileBase UploadImage)
        {
            LoadViewBags(model.CateId, model.ProviderId);
            ValidateDiscountDates(model);

            if (ModelState.IsValid)
            {
                string fileName = null;
                if (UploadImage != null)
                {
                    fileName = Path.GetFileName(UploadImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/image"), fileName);
                    UploadImage.SaveAs(path);
                }

                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Quantity = model.Quantity,
                    Photo = fileName,
                    Discount = model.Discount,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    ProviderId = model.ProviderId,
                    CateId = model.CateId,
                    Length = model.Length,
                    Width = model.Width,
                    Height = model.Height
                };

                db.Products.Add(product);
                db.SaveChanges();

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Show", new { CateId = model.CateId });
            }

            TempData["Error"] = "Thêm sản phẩm thất bại!";
            return View(model);
        }

        [HttpGet]
        [HasCredential(RoleId = "EDIT_PRODUCT")]
        public ActionResult Edit(int ProductId)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            var model = db.Products.Where(p => p.ProductId == ProductId)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Discount = p.Discount,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Photo = p.Photo,
                    ProviderId = p.ProviderId,
                    CateId = p.CateId,
                    Length = p.Length,
                    Width = p.Width,
                    Height = p.Height
                }).FirstOrDefault();

            if (model == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm!";
                return RedirectToAction("Index");
            }

            LoadViewBags(model.CateId, model.ProviderId);
            return View(model);
        }

        [HttpPost]
        [HasCredential(RoleId = "EDIT_PRODUCT")]
        public ActionResult Edit(ProductViewModel model, HttpPostedFileBase UploadImage)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            ViewBag.username = session.Username;

            LoadViewBags(model.CateId, model.ProviderId);
            ValidateDiscountDates(model);

            if (ModelState.IsValid)
            {
                var product = db.Products.FirstOrDefault(p => p.ProductId == model.ProductId);
                if (product != null)
                {
                    product.Name = model.Name;
                    product.Description = model.Description;
                    product.Price = model.Price;
                    product.Quantity = model.Quantity;
                    product.Discount = model.Discount;
                    product.StartDate = model.StartDate;
                    product.EndDate = model.EndDate;
                    product.ProviderId = model.ProviderId;
                    product.CateId = model.CateId;
                    product.Length = model.Length;
                    product.Width = model.Width;
                    product.Height = model.Height;

                    if (UploadImage != null)
                    {
                        if (!string.IsNullOrEmpty(product.Photo))
                        {
                            var oldImagePath = Path.Combine(Server.MapPath("~/image"), product.Photo);
                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }

                        string fileName = Path.GetFileName(UploadImage.FileName);
                        string path = Path.Combine(Server.MapPath("~/image"), fileName);
                        UploadImage.SaveAs(path);
                        product.Photo = fileName;
                    }

                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Show", new { CateId = model.CateId });
                }

                TempData["Error"] = "Không tìm thấy sản phẩm để cập nhật!";
            }

            return View(model);
        }

        private void ValidateDiscountDates(ProductViewModel model)
        {
            if (model.Discount > 0)
            {
                if (!model.StartDate.HasValue || !model.EndDate.HasValue)
                {
                    ModelState.AddModelError("", "Vui lòng nhập ngày bắt đầu và kết thúc khi có khuyến mãi.");
                }
                else if (model.StartDate > model.EndDate)
                {
                    ModelState.AddModelError("", "Ngày bắt đầu khuyến mãi phải nhỏ hơn ngày kết thúc.");
                }
            }
            else
            {
                if (model.StartDate.HasValue || model.EndDate.HasValue)
                {
                    ModelState.AddModelError("", "Không được nhập ngày nếu không có khuyến mãi.");
                }
            }
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
        [HttpPost]
        public ActionResult ToggleVisibility(int ProductId)
        {
            var product = db.Products.Find(ProductId);
            if (product != null)
            {
                product.IsVisible = !product.IsVisible;
                db.SaveChanges();
            }
            return RedirectToAction("Show");
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