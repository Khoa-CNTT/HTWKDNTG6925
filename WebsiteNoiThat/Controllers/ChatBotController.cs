using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Models.EF;

namespace WebsiteNoiThat.Controllers
{
    public class ChatBotController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage(string message)
        {
            // Debug: Thêm log để xem AI phân tích như nào
            var analysisResult = await AnalyzeMessageWithGemini(message);

            // Debug: Trả về thông tin phân tích để kiểm tra
            if (!analysisResult.IsProductRelated)
            {
                return Json(new
                {
                    reply = "Vui lòng hỏi về sản phẩm nội thất để tôi có thể hỗ trợ bạn tốt hơn.",
                    isNaturalReply = true,
                    debug = new
                    {
                        originalMessage = message,
                        analysisResult = analysisResult
                    }
                });
            }

            // Tìm kiếm sản phẩm dựa trên thông tin phân tích
            using (var db = new DBNoiThat())
            {
                var allProducts = db.Products.Where(p => p.IsVisible).ToList();
                var results = FilterProducts(allProducts, analysisResult, message);

                if (results.Count == 0)
                {
                    return Json(new { reply = "Không tìm thấy sản phẩm phù hợp. Vui lòng thử với tiêu chí khác.", isNaturalReply = true });
                }

                var productData = results.Select(p => new
                {
                    id = p.ProductId,
                    name = p.Name,
                    image = p.Photo ?? "placeholder.jpg",
                    price = p.Price.HasValue ? p.Price.Value.ToString("N0") : "Liên hệ",
                    description = p.Description,
                    dimensions = $"{p.Length}x{p.Width}x{p.Height}cm",
                    discount = p.Discount
                }).ToList();

                return Json(new
                {
                    reply = $"Tìm thấy {results.Count} sản phẩm phù hợp:",
                    products = productData,
                    isProductSuggestion = true
                });
            }
        }

        private System.Collections.Generic.List<Product> FilterProducts(System.Collections.Generic.List<Product> products, ProductAnalysisResult analysis, string originalMessage = "")
        {
            var query = products.AsQueryable();

            // Nếu có tin nhắn gốc, tìm kiếm trực tiếp trong tên và mô tả sản phẩm
            if (!string.IsNullOrEmpty(originalMessage))
            {
                var keywords = originalMessage.ToLower().Split(' ');

                query = query.Where(p =>
                    keywords.Any(keyword =>
                        keyword.Length > 2 && // Chỉ lấy từ có ít nhất 3 ký tự
                        (p.Name.ToLower().Contains(keyword) ||
                         (p.Description != null && p.Description.ToLower().Contains(keyword)))
                    )
                );
            }

            // Lọc theo loại sản phẩm (nếu có phân tích cụ thể)
            if (!string.IsNullOrEmpty(analysis.ProductType))
            {
                query = query.Where(p =>
                    p.Name.ToLower().Contains(analysis.ProductType) ||
                    (p.Description != null && p.Description.ToLower().Contains(analysis.ProductType)));
            }

            // Lọc theo màu sắc
            if (!string.IsNullOrEmpty(analysis.Color))
            {
                query = query.Where(p =>
                    p.Name.ToLower().Contains(analysis.Color) ||
                    (p.Description != null && p.Description.ToLower().Contains(analysis.Color)));
            }

            // Lọc theo giá
            if (analysis.MinPrice.HasValue)
            {
                query = query.Where(p => (p.Price ?? 0) >= analysis.MinPrice.Value * 1_000_000);
            }
            if (analysis.MaxPrice.HasValue)
            {
                query = query.Where(p => (p.Price ?? int.MaxValue) <= analysis.MaxPrice.Value * 1_000_000);
            }

            // Lọc theo kích thước
            if (analysis.MinLength.HasValue)
            {
                query = query.Where(p => p.Length >= analysis.MinLength.Value);
            }
            if (analysis.MaxLength.HasValue)
            {
                query = query.Where(p => p.Length <= analysis.MaxLength.Value);
            }
            if (analysis.MinWidth.HasValue)
            {
                query = query.Where(p => p.Width >= analysis.MinWidth.Value);
            }
            if (analysis.MaxWidth.HasValue)
            {
                query = query.Where(p => p.Width <= analysis.MaxWidth.Value);
            }
            if (analysis.MinHeight.HasValue)
            {
                query = query.Where(p => p.Height >= analysis.MinHeight.Value);
            }
            if (analysis.MaxHeight.HasValue)
            {
                query = query.Where(p => p.Height <= analysis.MaxHeight.Value);
            }

            // Sắp xếp theo độ ưu tiên
            var results = query.ToList();

            // Ưu tiên sản phẩm đang khuyến mại
            results = results.OrderByDescending(p => p.Discount ?? 0)
                           .ThenBy(p => p.Price ?? 0)
                           .Take(8)
                           .ToList();

            return results;
        }

        private ProductAnalysisResult AnalyzeMessageDirectly(string message)
        {
            if (string.IsNullOrEmpty(message))
                return new ProductAnalysisResult { IsProductRelated = false };

            string lowerMessage = message.ToLower();

            // Mở rộng danh sách từ khóa nội thất
            string[] furnitureKeywords = {
                "bàn", "ghế", "tủ", "giường", "kệ", "sofa", "nội thất", "furniture",
                "chair", "table", "bed", "cabinet", "shelf", "desk", "wardrobe",
                "bàn ăn", "bàn làm việc", "bàn học", "bàn coffee", "bàn trang điểm",
                "ghế ăn", "ghế làm việc", "ghế sofa", "ghế thư giãn",
                "tủ quần áo", "tủ bếp", "tủ sách", "tủ tivi", "tủ giày",
                "giường ngủ", "giường tầng", "giường sofa",
                "kệ sách", "kệ tivi", "kệ trang trí", "kệ để đồ",
                "sofa góc", "sofa đơn", "sofa giường",
                // Thêm các từ khóa về phòng và combo
                "phòng ngủ", "phòng khách", "phòng ăn", "phòng làm việc",
                "combo", "bộ", "set", "phòng", "nệm", "đệm",
                "moho", "mova", "furniland" // Thêm tên thương hiệu phổ biến
            };

            // Kiểm tra có chứa từ khóa nội thất không
            bool isProductRelated = furnitureKeywords.Any(keyword => lowerMessage.Contains(keyword));

            if (!isProductRelated)
                return new ProductAnalysisResult { IsProductRelated = false };

            // Phân tích cơ bản nếu có từ khóa
            var result = new ProductAnalysisResult { IsProductRelated = true };

            // Xác định loại sản phẩm (mở rộng thêm)
            if (lowerMessage.Contains("bàn")) result.ProductType = "bàn";
            else if (lowerMessage.Contains("ghế")) result.ProductType = "ghế";
            else if (lowerMessage.Contains("tủ")) result.ProductType = "tủ";
            else if (lowerMessage.Contains("giường")) result.ProductType = "giường";
            else if (lowerMessage.Contains("kệ")) result.ProductType = "kệ";
            else if (lowerMessage.Contains("sofa")) result.ProductType = "sofa";
            else if (lowerMessage.Contains("phòng ngủ")) result.ProductType = "phòng ngủ";
            else if (lowerMessage.Contains("phòng khách")) result.ProductType = "phòng khách";
            else if (lowerMessage.Contains("combo") || lowerMessage.Contains("bộ")) result.ProductType = "combo";

            // Xác định màu sắc
            string[] colors = { "trắng", "đen", "nâu", "gỗ", "xám", "xanh", "đỏ", "vàng", "hồng" };
            foreach (string color in colors)
            {
                if (lowerMessage.Contains(color))
                {
                    result.Color = color;
                    break;
                }
            }

            result.SearchIntent = "Tìm kiếm sản phẩm nội thất";
            return result;
        }

        private async Task<ProductAnalysisResult> AnalyzeMessageWithGemini(string message)
        {
            // Fallback: Kiểm tra từ khóa trực tiếp trước khi gọi AI
            var directAnalysis = AnalyzeMessageDirectly(message);
            if (directAnalysis.IsProductRelated)
            {
                return directAnalysis;
            }

            string apiKey = "AIzaSyCx25msrRJa6cs-43QkXz4U_q38XmdBa_w";
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";

            string instruction = $@"
PHÂN TÍCH câu hỏi về nội thất và trả về JSON:

Câu hỏi: '{message}'

Nếu có chứa bất kỳ từ nào liên quan đến nội thất (bàn, ghế, tủ, giường, kệ, sofa, nội thất, furniture, combo, phòng ngủ, phòng khách) → isProductRelated: true

JSON format:
{{
  ""isProductRelated"": true/false,
  ""productType"": ""tên loại sản phẩm"",
  ""color"": ""màu sắc"",
  ""minPrice"": số_triệu,
  ""maxPrice"": số_triệu,
  ""minLength"": số_cm,
  ""maxLength"": số_cm,
  ""minWidth"": số_cm,
  ""maxWidth"": số_cm,
  ""minHeight"": số_cm,
  ""maxHeight"": số_cm,
  ""searchIntent"": ""ý định tìm kiếm""
}}

CHỈ trả về JSON:
";

            var requestBody = new
            {
                contents = new[] {
                    new {
                        parts = new[] {
                            new { text = instruction }
                        }
                    }
                }
            };

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new ProductAnalysisResult { IsProductRelated = false };

                try
                {
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    string jsonText = result.candidates[0].content.parts[0].text;

                    // Làm sạch JSON (loại bỏ markdown formatting nếu có)
                    jsonText = jsonText.Replace("```json", "").Replace("```", "").Trim();

                    if (!jsonText.StartsWith("{"))
                        return new ProductAnalysisResult { IsProductRelated = false };

                    dynamic parsed = JsonConvert.DeserializeObject(jsonText);

                    return new ProductAnalysisResult
                    {
                        IsProductRelated = parsed.isProductRelated ?? false,
                        ProductType = (parsed.productType ?? "").ToString().ToLower(),
                        Color = (parsed.color ?? "").ToString().ToLower(),
                        MinPrice = parsed.minPrice != null ? (int?)parsed.minPrice : null,
                        MaxPrice = parsed.maxPrice != null ? (int?)parsed.maxPrice : null,
                        MinLength = parsed.minLength != null ? (decimal?)parsed.minLength : null,
                        MaxLength = parsed.maxLength != null ? (decimal?)parsed.maxLength : null,
                        MinWidth = parsed.minWidth != null ? (decimal?)parsed.minWidth : null,
                        MaxWidth = parsed.maxWidth != null ? (decimal?)parsed.maxWidth : null,
                        MinHeight = parsed.minHeight != null ? (decimal?)parsed.minHeight : null,
                        MaxHeight = parsed.maxHeight != null ? (decimal?)parsed.maxHeight : null,
                        SearchIntent = (parsed.searchIntent ?? "").ToString()
                    };
                }
                catch
                {
                    return new ProductAnalysisResult { IsProductRelated = false };
                }
            }
        }
    }

    public class ProductAnalysisResult
    {
        public bool IsProductRelated { get; set; }
        public string ProductType { get; set; }
        public string Color { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public decimal? MinLength { get; set; }
        public decimal? MaxLength { get; set; }
        public decimal? MinWidth { get; set; }
        public decimal? MaxWidth { get; set; }
        public decimal? MinHeight { get; set; }
        public decimal? MaxHeight { get; set; }
        public string SearchIntent { get; set; }
    }
}