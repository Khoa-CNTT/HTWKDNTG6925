using Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteNoiThat.Models;

namespace WebsiteNoiThat.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private DBNoiThat db = new DBNoiThat();

        // GET: Admin/Home
        public ActionResult Index()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
            if (session != null)
            {
                ViewBag.username = session.Username;

                // Thống kê tổng quan
                ViewBag.TotalOrders = db.Orders.Count();
                // Tính tổng doanh thu từ OrderDetail
                decimal totalRevenue = db.OrderDetails
    .Where(od => db.Orders.Any(o => o.OrderId == od.OrderId && o.StatusId == 5)) // Đã thanh toán
    .ToList()
    .Sum(od => (od.Price ?? 0) * (od.Quantity ?? 0));

                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.TotalCustomers = db.Users.Count(u => u.GroupId == "USER");
                ViewBag.TotalProducts = db.Products.Count();

                // Dữ liệu cho biểu đồ tròn thống kê đơn hàng
                var orderStatusData = new List<int>
                {
                    db.Orders.Count(o => o.StatusId == 1), // Đã tiếp nhận
                    db.Orders.Count(o => o.StatusId == 2), // Đã xử lý
                    db.Orders.Count(o => o.StatusId == 3), // Đã huỷ
                    db.Orders.Count(o => o.StatusId == 4), // Đang giao hàng
                    db.Orders.Count(o => o.StatusId == 5), // Đã thanh toán
                    db.Orders.Count(o => o.StatusId == 6)  // Đã giao
                };
                ViewBag.OrderStatusData = orderStatusData;

                // Dữ liệu cho biểu đồ cột thống kê người dùng
                var userData = new List<int>
                {
                    db.Users.Count(u => u.GroupId == "ADMIN"), // Admin
                    db.Users.Count(u => u.GroupId == "MOD"),   // Mod
                    db.Users.Count(u => u.GroupId == "USER")   // User
                };
                ViewBag.UserData = userData;

                return View();
            }
            else
            {
                return Redirect("~/Admin/Login");
            }
        }
        //public ActionResult Show()
        //{
        //    var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion_admin];
        //    if (session != null)
        //    {
        //        ViewBag.username = session.Username;
        //        return View();

        //    }
        //    else
        //    {
        //        return Redirect("~/Admin/Login");
        //    }
        //}

    }
}