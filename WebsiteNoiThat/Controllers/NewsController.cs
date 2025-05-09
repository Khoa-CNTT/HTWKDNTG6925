using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models.DAO;
using Models.EF;

namespace WebsiteNoiThat.Controllers
{
    public class NewsController : Controller
    {
        // GET: News
        DBNoiThat db = new DBNoiThat();
        public ActionResult NewsHot()
        {
            var model = new NewsDao().NewsHot();
            return PartialView(model);
        }
        public ActionResult Show(int NewsId)
        {
            var news = db.News.SingleOrDefault(n => n.NewsId == NewsId);

            // Lấy 3 tin liên quan khác (trừ tin đang xem), mới nhất
            var relatedNews = db.News
                .Where(n => n.NewsId != NewsId)
                .OrderByDescending(n => n.DateUpdate)
                .Take(3)
                .ToList();

            ViewBag.RelatedNews = relatedNews;

            return View(news);
        }

        public ActionResult View()
        {
            var model = db.News
                          .OrderByDescending(n => n.DateUpdate) // Sắp xếp theo ngày cập nhật mới nhất
                          .ToList();

            return View(model);
        }


    }
}