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
            var (productType, color, minPrice, maxPrice, length, width, height) = await ExtractKeywordsWithGemini(message);

            // Fallback: nếu message chứa từ khóa sản phẩm thì gán productType
            if (string.IsNullOrEmpty(productType))
            {
                string[] knownTypes = { "tủ", "bàn", "ghế", "sofa", "kệ", "giường" };
                foreach (var type in knownTypes)
                {
                    if (message.ToLower().Contains(type))
                    {
                        productType = type;
                        break;
                    }
                }
            }

            // Kiểm tra có phải tìm sản phẩm không
            bool isProductRelated =
                (!string.IsNullOrEmpty(productType)) ||
                !string.IsNullOrEmpty(color) || minPrice != null || maxPrice != null || length != null || width != null || height != null;

            if (!isProductRelated)
            {
                string reply = await GetNaturalReplyFromGemini(message);
                return Json(new { reply = reply });
            }

            using (var db = new DBNoiThat())
            {
                var allProducts = db.Products.ToList();

                var results = allProducts
                    .Where(p =>
                        (string.IsNullOrEmpty(productType) || $"{p.Name} {p.Description}".ToLower().Contains(productType)) &&
                        (string.IsNullOrEmpty(color) || $"{p.Name} {p.Description}".ToLower().Contains(color)) &&
                        (!minPrice.HasValue || (p.Price ?? 0) >= minPrice.Value * 1_000_000) &&
                        (!maxPrice.HasValue || (p.Price ?? int.MaxValue) <= maxPrice.Value * 1_000_000) &&
                        (!length.HasValue || p.Length >= length.Value) &&
                        (!width.HasValue || p.Width >= width.Value) &&
                        (!height.HasValue || p.Height >= height.Value)
                    )
                    .Take(6)
                    .Select(p => new
                    {
                        id = p.ProductId,
                        name = p.Name,
                        image = p.Photo ?? "placeholder.jpg",
                        price = p.Price.HasValue ? p.Price.Value.ToString("N0") : "Liên hệ"
                    })
                    .ToList();

                string json = JsonConvert.SerializeObject(results);
                return Json(new { reply = json });
            }
        }

        private async Task<(string productType, string color, int? minPrice, int? maxPrice, decimal? length, decimal? width, decimal? height)>
            ExtractKeywordsWithGemini(string prompt)
        {
            string apiKey = "AIzaSyCx25msrRJa6cs-43QkXz4U_q38XmdBa_w";
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";

            string instruction = $@"
Bạn là một trợ lý tư vấn nội thất chuyên nghiệp.

Nhiệm vụ:
1. Đọc câu hỏi của khách hàng.
2. Phân tích và trích xuất các thông tin tìm kiếm liên quan đến sản phẩm nội thất.

Trả kết quả ở dạng JSON với đúng 7 trường sau (giữ nguyên thứ tự):

{{
  ""productType"": string hoặc null,   // ví dụ: 'tủ', 'bàn', 'sofa'
  ""color"": string hoặc null,         // màu sắc, ví dụ: 'trắng', 'đen'
  ""minPrice"": int hoặc null,         // giá thấp nhất (triệu VNĐ)
  ""maxPrice"": int hoặc null,         // giá cao nhất (triệu VNĐ)
  ""length"": decimal hoặc null,       // chiều dài tính bằng cm
  ""width"": decimal hoặc null,        // chiều rộng tính bằng cm
  ""height"": decimal hoặc null        // chiều cao tính bằng cm
}}

❗ Không thêm bất kỳ lời giải thích, văn bản hoặc nhận xét nào khác.

Câu hỏi của khách: '{prompt}'
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
                    return ("", "", null, null, null, null, null);

                try
                {
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    string jsonText = result.candidates[0].content.parts[0].text;

                    if (!jsonText.Trim().StartsWith("{"))
                        return ("", "", null, null, null, null, null); // không đúng format

                    dynamic parsed = JsonConvert.DeserializeObject(jsonText);

                    string productType = parsed.productType ?? "";
                    string color = parsed.color ?? "";
                    int? minPrice = parsed.minPrice != null ? (int?)parsed.minPrice : null;
                    int? maxPrice = parsed.maxPrice != null ? (int?)parsed.maxPrice : null;
                    decimal? length = parsed.length != null ? (decimal?)parsed.length : null;
                    decimal? width = parsed.width != null ? (decimal?)parsed.width : null;
                    decimal? height = parsed.height != null ? (decimal?)parsed.height : null;

                    return (
                        productType.ToLower(),
                        color.ToLower(),
                        minPrice, maxPrice,
                        length, width, height
                    );
                }
                catch
                {
                    return ("", "", null, null, null, null, null);
                }
            }
        }

        private async Task<string> GetNaturalReplyFromGemini(string prompt)
        {
            string apiKey = "AIzaSyCx25msrRJa6cs-43QkXz4U_q38XmdBa_w";
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[] {
                    new {
                        parts = new[] {
                            new { text = prompt }
                        }
                    }
                }
            };

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                try
                {
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    return result.candidates[0].content.parts[0].text ?? "Xin lỗi, tôi chưa hiểu ý bạn.";
                }
                catch
                {
                    return "Xin lỗi, tôi chưa hiểu ý bạn.";
                }
            }
        }
    }
}
