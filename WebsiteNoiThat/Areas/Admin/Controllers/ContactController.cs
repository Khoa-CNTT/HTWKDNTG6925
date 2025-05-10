using Models.EF;
using System;
using System.Linq;
using System.Web.Mvc;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class ContactController : Controller
    {
        DBNoiThat db = new DBNoiThat();

        public ActionResult Index()
        {
            var model = db.Contacts.OrderByDescending(x => x.CreatedAt).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(FormCollection formCollection)
        {
            string[] ids = formCollection["ContactId"].Split(new char[] { ',' });

            foreach (string id in ids)
            {
                if (int.TryParse(id, out int contactId))
                {
                    var model = db.Contacts.Find(contactId);
                    if (model != null)
                    {
                        db.Contacts.Remove(model);
                        db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("Index");
        }
    }
}
