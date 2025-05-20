
# 🛋️ Hệ Thống Website Kinh Doanh Nội Thất Gia Đình Thông Minh 💡

Một nền tảng thương mại điện tử nội thất tích hợp **AI Chatbot**, hỗ trợ người dùng và quản trị viên quản lý toàn diện quy trình kinh doanh, từ mua sắm đến hậu mãi và phân quyền hệ thống.

> 🚀 Dự án được xây dựng bởi nhóm sinh viên Khoa Công Nghệ Thông Tin 

---

## 🌟 Tính Năng Nổi Bật

### 👤 Dành Cho Người Dùng

- 🔐 Đăng ký, Đăng nhập, Quên mật khẩu
- 🛒 Giỏ hàng, Thanh toán đơn hàng
- 🧾 Xem **lịch sử mua hàng**, cập nhật **thông tin cá nhân**
- 🕵️‍♂️ Tìm kiếm sản phẩm theo danh mục, giá cả, từ khóa
- 🧠 Chatbot AI tư vấn 
- 📦 Xem **chi tiết sản phẩm** (hình ảnh, kích thước, chất liệu, giá cả)
- 🎁 Đăng ký **thẻ tích điểm khách hàng thân thiết**
- 📬 Liên hệ / Gửi phản hồi
- 🔓 Đăng xuất an toàn

### 🛠️ Dành Cho Quản Trị Viên (Admin)

- 👨‍💼 Quản lý thông tin cá nhân của quản trị viên
- 📰 Quản lý **tin tức**, nội dung truyền thông
- 🛋️ Quản lý **sản phẩm** và **loại sản phẩm**
- 🤝 Quản lý **nhà cung cấp**
- 📦 Quản lý **đơn đặt hàng**
- 📊 Thống kê – Báo cáo tổng quan hoạt động
- 👥 Quản lý **khách hàng**, **nhân viên**
- 🔐 Phân quyền người dùng:
  - Theo **quyền hạn**
  - Theo **nhóm người dùng**
  - Theo **nhóm chức năng**
- 📨 Quản lý các yêu cầu **liên hệ từ người dùng**

---

## 🧠 Công Nghệ Sử Dụng

| Thành phần      | Công nghệ sử dụng                    |
|------------------|-------------------------------------|
| Frontend         | HTML, CSS, JavaScript, Bootstrap    |
| Backend          | ASP.NET MVC (.NET Framework 4.8)    |
| Cơ sở dữ liệu    | SQL Server                          |
| AI Chatbot       | Dialogflow hoặc Gemini AI           |
| IDE              | Visual Studio 2019+                 |

---

## 🗂️ Cấu Trúc Thư Mục

```
HTWKDNTG6925/
├── WebsiteNoiThat/         # Giao diện và chức năng website
├── Models/                 # Các lớp mô hình dữ liệu
├── Controllers/            # Xử lý điều hướng và logic
├── Views/                  # Giao diện người dùng
├── Scripts/                # JavaScript, chatbot, xử lý client
├── wwwroot/                # Tài nguyên tĩnh
├── App_Data/               # Cơ sở dữ liệu mẫu
└── WebsiteNoiThat.sln      # Tệp giải pháp (Solution) cho Visual Studio
```

---

## 🛠️ Hướng Dẫn Cài Đặt Dự Án

1. **Clone repository**:
   ''' bash
   git clone https://github.com/Khoa-CNTT/HTWKDNTG6925.git
   ```
2. **Mở bằng Visual Studio 2019+**:
   - Mở file `WebsiteNoiThat.sln`
3. **Cấu hình cơ sở dữ liệu** trong `web.config`
4. **Khôi phục gói NuGet**
5. **Chạy dự án (F5)**

---

## 🤖 Hỗ Trợ Tích Hợp Chatbot

- Chatbot AI được xây dựng giúp:
  - Gợi ý sản phẩm thông minh
  - Trả lời câu hỏi thường gặp

---

## 📊 Mô Hình CSDL (Tóm tắt)
'''

User - Role - UserGroup - Product - Category - Order - OrderDetail - Status - Contact - ChatMessage - News - Provider - Credential - Card
```



> 📌 *Được thực hiện bởi nhóm sinh viên ngành Công nghệ Thông tin 
