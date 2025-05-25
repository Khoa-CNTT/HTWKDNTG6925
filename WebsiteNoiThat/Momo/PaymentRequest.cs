using System;
using System.IO;
using System.Net;
using System.Text;

namespace WebsiteNoiThat.Momo
{
    class PaymentRequest
    {
        public static string sendPaymentRequest(string endpoint, string postJsonString)
        {
            try
            {
                // Quan trọng: Thiết lập TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var request = (HttpWebRequest)WebRequest.Create(endpoint);
                byte[] data = Encoding.UTF8.GetBytes(postJsonString);

                request.ProtocolVersion = HttpVersion.Version11;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;
                request.ReadWriteTimeout = 30000;
                request.Timeout = 15000;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                // Cố gắng đọc nội dung lỗi từ response nếu có
                using (var response = ex.Response as HttpWebResponse)
                {
                    if (response != null)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            return $"Error (Response): {reader.ReadToEnd()}";
                        }
                    }
                }

                // Nếu không có response từ server
                return $"WebException: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
    }
}
