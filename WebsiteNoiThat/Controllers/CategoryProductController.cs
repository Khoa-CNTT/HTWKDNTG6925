using Models.EF;
using Models.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PagedList;

namespace WebsiteNoiThat.Controllers
{
    public class CategoryProductController : Controller

    {
        private DBNoiThat db = new DBNoiThat(); 
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Menu()
        {
            var model = new CategoryDao().ListCategory();
            return PartialView(model);
        }

        public ActionResult CategoryShow(int id, int page = 1, int pagesize = 12)
        {
            // Lấy danh mục theo id
            var category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound(); 
            }

            ViewBag.CategoryShow = category;

            var subCategoryIds = db.Categories
                .Where(c => c.ParId == id)
                .Select(c => c.CategoryId)
                .ToList();

            subCategoryIds.Add(id); 

            var model = db.Products
                .Where(p => p.IsVisible && subCategoryIds.Contains(p.CateId ?? 0))
                .OrderByDescending(x => x.ProductId)
                .ToPagedList(page, pagesize);

            ViewBag.Totalpage = model.PageCount;
            ViewBag.Page = page;
            ViewBag.Maxpage = 5;
            ViewBag.First = 1;
            ViewBag.Last = model.PageCount;
            ViewBag.Next = page + 1;
            ViewBag.Prev = page - 1;

            return View(model.ToList());
        }




        private List<int> GetAllChildCategoryIds(int parentId, DBNoiThat db)
        {
            var result = new List<int> { parentId };

            var children = db.Categories.Where(c => c.ParId == parentId).Select(c => c.CategoryId).ToList();

            foreach (var childId in children)
            {
                result.AddRange(GetAllChildCategoryIds(childId, db)); // đệ quy lấy cháu chắt
            }

            return result;
        }


    }
}
