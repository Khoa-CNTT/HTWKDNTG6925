
using Models.DAO;
using Models.EF;
using Newtonsoft.Json.Linq;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebsiteNoiThat.Common;
using WebsiteNoiThat.Models;
using WebsiteNoiThat.Momo;
using WebsiteNoiThat.VNPay;

namespace WebsiteNoiThat.Controllers
{
    public class CartController : Controller
    {

        DBNoiThat db = new DBNoiThat();
        private const string CartSession = "CartSession";
        private const string HistorySession = "HistorySession";

        public ActionResult Index()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            if (session != null)
            {
                var cart = Session[CartSession];
                var list = new List<CartItem>();
                if (cart != null)
                {
                    ViewBag.Status = "Đang chờ xác nhận";
                    list = (List<CartItem>)cart;
                }
                return View(list);
            }
            else
            {
                return Redirect("/dang-nhap");
            }
        }

        public JsonResult DeleteAll()
        {
            Session[CartSession] = null;
            return Json(new
            {
                status = true
            });
        }

        public JsonResult Delete(long id)
        {
            var sessionCart = (List<CartItem>)Session[CartSession];
            sessionCart.RemoveAll(x => x.Product.ProductId == id);
            Session[CartSession] = sessionCart;
            return Json(new
            {
                status = true
            });
        }
        public ActionResult DeleteItem(long id)
        {
            var model = db.OrderDetails.SingleOrDefault(n => n.OrderDetailId == id);
            if (model == null)
            {
                return HttpNotFound();
            }

            var order = db.Orders.SingleOrDefault(o => o.OrderId == model.OrderId);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (order.StatusId == 1 || order.StatusId == 2) // Chỉ cho huỷ khi đang xử lý hoặc mới đặt
            {
                // Hoàn trả số lượng sản phẩm về kho
                var product = db.Products.SingleOrDefault(p => p.ProductId == model.ProductId);
                if (product != null)
                {
                    product.Quantity += model.Quantity; // Cộng lại số lượng vào kho
                }

                order.StatusId = 3; // Gán trạng thái huỷ
                db.SaveChanges();
            }
            else
            {
                return Redirect("/loi-huy-hang");
            }

            return RedirectToAction("HistoryCart");
        }


        public JsonResult Update(string cartModel)
        {
            var jsonCart = new JavaScriptSerializer().Deserialize<List<CartItem>>(cartModel);
            var sessionCart = (List<CartItem>)Session[CartSession];
            foreach (var item in sessionCart)
            {
                var jsonItem = jsonCart.SingleOrDefault(x => x.Product.ProductId == item.Product.ProductId);
                if (jsonItem != null)
                {
                    item.Quantity = jsonItem.Quantity;
                }
            }
            Session[CartSession] = sessionCart;
            return Json(new
            {
                status = true
            });
        }
        public ActionResult AddCart(int productId, int quantity)
        {
            var product = new ProductDao().ViewDetail(productId);
            var cart = Session[CartSession];
            if (cart != null)
            {
                var list = (List<CartItem>)cart;
                if (list.Exists(x => x.Product.ProductId == productId))
                {
                    foreach (var item in list)
                    {
                        if (item.Product.ProductId == productId)
                        {
                            item.Quantity += quantity;
                        }
                    }
                }
                else
                {
                    //tạo mới đối tượng cart item
                    var item = new CartItem();
                    item.Product = product;
                    item.Quantity = quantity;
                    list.Add(item);
                }
                //Gán vào session
                Session[CartSession] = list;
                //Session[HistorySession] = list;
            }
            else
            {
                //tạo mới đối tượng cart item
                var item = new CartItem();
                item.Product = product;
                item.Quantity = quantity;
                var list = new List<CartItem>();
                list.Add(item);
                //Gán vào session
                Session[CartSession] = list;
                //Session[HistorySession] = list;
            }
            return RedirectToAction("Index");
        }
        private long InsertOrder(User n, string paymentMethod, List<CartItem> cart, double total)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            var order = new Order
            {
                UpdateDate = DateTime.Now,
                ShipAddress = n.Address,
                ShipPhone = n.Phone,
                ShipName = n.Name,
                ShipEmail = n.Email,
                PaymentMethod = paymentMethod,
                UserId = session.UserId,
                StatusId = 1
            };

            var id = new OrderDao().Insert(order);
            var detailDao = new OrderDetailDao();

            // Gửi email
            string content = System.IO.File.ReadAllText(Server.MapPath("~/Common/neworder.html"));

            content = content.Replace("{{id}}", id.ToString());
            content = content.Replace("{{OrderTime}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            content = content.Replace("{{CustomerName}}", n.Name);
            content = content.Replace("{{Phone}}", n.Phone.ToString());
            content = content.Replace("{{Email}}", n.Email);
            content = content.Replace("{{Address}}", n.Address);
            content = content.Replace("{{Total}}", total.ToString("N0"));

            // Tạo bảng HTML chi tiết sản phẩm
            string tableRows = "";
            int count = 1;

            foreach (var item in cart)
            {
                var discountprice = Convert.ToInt32(item.Product.Price - item.Product.Price * item.Product.Discount * 0.01);

                // Trừ kho hàng
                var pro = db.Products.FirstOrDefault(m => m.ProductId == item.Product.ProductId);
                if (pro != null)
                {
                    pro.Quantity -= item.Quantity;
                }

                // Lưu chi tiết đơn hàng
                detailDao.Insert(new OrderDetail
                {
                    OrderId = id,
                    ProductId = item.Product.ProductId,
                    Price = discountprice,
                    Quantity = item.Quantity
                });

                db.SaveChanges();

                tableRows += $@"
            <tr style='border-bottom: 1px solid #ddd;'>
                <td style='padding: 10px; text-align: center;'>{count}</td>
                <td style='padding: 10px;'>{item.Product.Name}</td>
                <td style='padding: 10px; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 10px; text-align: right;'>{discountprice.ToString("N0")} ₫</td>
                <td style='padding: 10px; text-align: center;'>{item.Product.Discount}%</td>
            </tr>";
                count++;
            }

            string htmlTable = $@"
        <table style='width: 100%; border-collapse: collapse; font-family: Arial, sans-serif; margin-top: 20px;'>
            <thead style='background-color: #f2f2f2;'>
                <tr>
                    <th style='padding: 12px; border-bottom: 2px solid #ddd;'>STT</th>
                    <th style='padding: 12px; border-bottom: 2px solid #ddd;'>Tên sản phẩm</th>
                    <th style='padding: 12px; border-bottom: 2px solid #ddd;'>Số lượng</th>
                    <th style='padding: 12px; border-bottom: 2px solid #ddd;'>Đơn giá</th>
                    <th style='padding: 12px; border-bottom: 2px solid #ddd;'>Khuyến mãi</th>
                </tr>
            </thead>
            <tbody>
                {tableRows}
            </tbody>
        </table>";

            content = content.Replace("{{data}}", htmlTable);

            string toEmail = ConfigurationManager.AppSettings["ToEmailAddress"]; // nội bộ nếu cần

            new MailHelper().SendMail(n.Email, "Xác nhận đơn hàng từ Nội Thất Seven", content);
            // new MailHelper().SendMail(toEmail, "Đơn hàng mới từ Nội Thất Seven", content);

            return id;
        }


        [HttpGet]
        public ActionResult PayBy()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            if (session != null)
            {
                var model = db.Users.SingleOrDefault(n => n.UserId == session.UserId);
                var cart = Session[CartSession];
                var list = new List<CartItem>();
                var total = 0;
                if (cart != null)
                {
                    ViewBag.Status = "Đang chờ xác nhận";
                    list = (List<CartItem>)cart;

                    foreach (CartItem item in list)
                    {
                        total = total + Convert.ToInt32(item.Product.Price * item.Quantity - item.Product.Price * item.Product.Discount * 0.01 * item.Quantity);
                    }
                }
                ViewBag.ListItem = list;
                ViewBag.total = total;

                return View(model);
            }
            else
            {
                return Redirect("/dang-nhap");
            }
        }

        [HttpPost]
        public ActionResult PayBy(User n, string PaymentMethod)
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            var model = db.Users.SingleOrDefault(a => a.UserId == session.UserId);

            model.Name = n.Name;
            model.Phone = n.Phone;
            model.Address = n.Address;
            model.Email = n.Email;
            model.Status = true;

            db.Entry(model).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            var cart = (List<CartItem>)Session[CartSession];
            double total = 0;

            foreach (var item in cart)
            {
                var discountprice = item.Product.Price - item.Product.Price * item.Product.Discount * 0.01;
                total += (double)(discountprice * item.Quantity);
            }

            if (PaymentMethod == "VNPAY")
            {
                Session["CartForVnp"] = cart;
                Session["UserForVnp"] = n;
                Session["TotalForVnp"] = total;
                return RedirectToAction("PaymentVNPay");
            }
            if (PaymentMethod == "Momo")
            {
                Session["CartForMomo"] = cart;
                Session["UserForMomo"] = n;
                Session["TotalForMomo"] = total;
                return RedirectToAction("PaymentMomo");
            }

            else
            {
                // Thanh toán COD hoặc Momo
                var id = InsertOrder(n, PaymentMethod, cart, total);
                return Redirect("/hoan-thanh");
            }
        }
        public ActionResult PaymentVNPay()
        {
            // cấu hình VNPAY
            var n = Session["UserForVnp"] as User;
            var cart = Session["CartForVnp"] as List<CartItem>;
            var total = Convert.ToDouble(Session["TotalForVnp"]);


            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];

            PayLib pay = new PayLib();
            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            pay.AddRequestData("vnp_Amount", ((int)total * 100).ToString());
            pay.AddRequestData("vnp_BankCode", "");
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress());
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng Nội Thất Seven");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

            TempData["Cart"] = cart;
            TempData["User"] = n;
            TempData["Total"] = total;

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);
            return Redirect(paymentUrl);
        }
        public ActionResult PaymentVNPConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"];
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                        pay.AddResponseData(s, vnpayData[s]);
                }

                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode");
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret);

                // Lấy dữ liệu từ session
                var user = Session["UserForVnp"] as User;
                var cart = Session["CartForVnp"] as List<CartItem>;
                var total = Session["TotalForVnp"] != null ? Convert.ToDouble(Session["TotalForVnp"]) : 0;

                if (checkSignature && vnp_ResponseCode == "00" && user != null && cart != null)
                {
                    InsertOrder(user, "VNPAY", cart, total);
                    ViewBag.Message = "Thanh toán thành công!";
                    ViewBag.PaymentMethod = "VNPAY";
                    ViewBag.Status = "Đã thanh toán";
                    Session["CartSession"] = null;
                    // Xoá session sau khi xử lý
                    Session.Remove("UserForVnp");
                    Session.Remove("CartForVnp");
                    Session.Remove("TotalForVnp");

                    return View("PaymentVNPConfirm", cart);
                }
                else
                {
                    ViewBag.Message = "Thanh toán thất bại hoặc dữ liệu không hợp lệ.";
                    return RedirectToAction("Index");

                }
            }

            ViewBag.Message = "Không có dữ liệu phản hồi từ cổng thanh toán.";
            return RedirectToAction("Index");

        }
        public ActionResult PaymentMomo()
        {
            var n = Session["UserForMomo"] as User;
            var cart = Session["CartForMomo"] as List<CartItem>;
            var total = Convert.ToDouble(Session["TotalForMomo"]);

            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMOJMUD20220907";
            string accessKey = "4P1sX4jWK4RhExaX";
            string serectkey = "FcnI5hgWY9fkaf5rNRNrR8Ugrq7LaRCw";

            string orderInfo = "Thanh toán đơn hàng Nội Thất Seven";
            string returnUrl = "http://localhost:58473/Cart/ConfirmPaymentMomo"; // sửa theo domain thật khi deploy
            string notifyurl = "https://dashboard.ngrok.com/events/subscriptions"; // test dùng ngrok hoặc webhook.site

            string amount = ((int)total).ToString();
            string orderid = DateTime.Now.Ticks.ToString();
            string requestId = DateTime.Now.Ticks.ToString();
            string extraData = "";

            string rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderId={orderid}&orderInfo={orderInfo}&returnUrl={returnUrl}&notifyUrl={notifyurl}&extraData={extraData}";

            MoMoSecurity crypto = new MoMoSecurity();
            string signature = crypto.signSHA256(rawHash, serectkey);

            JObject message = new JObject
    {
        { "partnerCode", partnerCode },
        { "accessKey", accessKey },
        { "requestId", requestId },
        { "amount", amount },
        { "orderId", orderid },
        { "orderInfo", orderInfo },
        { "returnUrl", returnUrl },
        { "notifyUrl", notifyurl },
        { "extraData", extraData },
        { "requestType", "captureMoMoWallet" },
        { "signature", signature }
    };

            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());
            JObject jmessage = JObject.Parse(responseFromMomo);
            return Redirect(jmessage.GetValue("payUrl").ToString());
        }
        public ActionResult ConfirmPaymentMomo()
        {
            string errorCode = Request.QueryString["errorCode"];
            var user = Session["UserForMomo"] as User;
            var cart = Session["CartForMomo"] as List<CartItem>;
            var total = Session["TotalForMomo"] != null ? Convert.ToDouble(Session["TotalForMomo"]) : 0;

            if (errorCode == "0" && user != null && cart != null)
            {
                InsertOrder(user, "Momo", cart, total);
                ViewBag.Message = "Thanh toán qua Momo thành công!";
                Session["CartSession"] = null;
                ViewBag.PaymentMethod = "Momo";
                ViewBag.Status = "Đã thanh toán";

                Session.Remove("UserForMomo");
                Session.Remove("CartForMomo");
                Session.Remove("TotalForMomo");

                return View("PaymentVNPConfirm", cart); // Reuse view

            }
            else
            {
                ViewBag.Message = "Thanh toán qua Momo thất bại hoặc bị huỷ.";
                return RedirectToAction("Index");
            }
        }

        public ActionResult HistoryCart(int? page)
        {
            int pagenumber = (page ?? 1);
            int pagesize = 6;
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];

            var model = (from a in db.OrderDetails
                         join b in db.Orders
                         on a.OrderId equals b.OrderId
                         join c in db.Products
                         on a.ProductId equals c.ProductId
                         join d in db.Status on b.StatusId equals d.StatusId
                         where b.UserId == session.UserId

                         select new HistoryCart
                         {
                             OrderDetaiId = a.OrderDetailId,
                             ProductId = a.ProductId,
                             Name = c.Name,
                             Photo = c.Photo,
                             Price = a.Price,
                             Quantity = a.Quantity,
                             Discount = c.Discount,
                             EndDate = c.EndDate,
                             StatusId = b.StatusId,
                             PaymentMethod = b.PaymentMethod,
                             NameStatus = d.Name
                         }).ToList();

            return View(model.ToPagedList(pagenumber, pagesize));

        }

        public ActionResult Success()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];
            var cart = Session[CartSession];
            var list = new List<CartItem>();
            if (cart != null)
            {
                list = (List<CartItem>)cart;
                ViewBag.Status = "Đã tiếp nhận";
                ViewBag.ListItem = list;
                Session["CartSession"] = null;
            }
            return View(list);
        }

        public ActionResult Error()
        {
            return View();
        }
        public ActionResult DeleteError()
        {
            var session = (UserLogin)Session[WebsiteNoiThat.Common.Commoncontent.user_sesion];

            var model = (from a in db.OrderDetails
                         join b in db.Orders
                         on a.OrderId equals b.OrderId
                         join c in db.Products
                         on a.ProductId equals c.ProductId
                         join d in db.Status on b.StatusId equals d.StatusId
                         where b.UserId == session.UserId

                         select new HistoryCart
                         {
                             OrderDetaiId = a.OrderDetailId,
                             ProductId = a.ProductId,
                             Name = c.Name,
                             Photo = c.Photo,
                             Price = a.Price,
                             Quantity = a.Quantity,
                             Discount = c.Discount,
                             StatusId = b.StatusId,
                             NameStatus = d.Name
                         }).ToList();

            return View(model);
        }
    }
}