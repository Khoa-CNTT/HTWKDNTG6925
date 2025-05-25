using Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Common;

namespace WebsiteNoiThat.Controllers
{
    public class ReviewController : Controller
    {
        DBNoiThat db = new DBNoiThat();
        public ActionResult GetReviewsByProduct(int productId)
        {
            var reviews = db.Reviews
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            return Json(reviews, JsonRequestBehavior.AllowGet);
        }
       
    }
}