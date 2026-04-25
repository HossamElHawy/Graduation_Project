using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace IPS_PROJECT.Services
{
    public class AiPredictionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiPredictionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetRawPredictionAsync(object features)
        {
            // 1. استخدام البورت الجديد 8000 مع الحفاظ على إمكانية القراءة من appsettings
            var endpoint = _configuration["AiSettings:Endpoint"] ?? "http://127.0.0.1:8000/predict";
            var apiKey = _configuration["AiSettings:ApiKey"] ?? "";

            try
            {
                // 2. تعديل الـ Payload ليتوافق مع FastAPI (تغليف الخصائص داخل كائن features)
                // الموديل الجديد يتوقع: { "features": { ... } }
                var payload = new { features = features };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // 3. إضافة الـ API Key إذا كان موجوداً (للأمان)
                if (!string.IsNullOrEmpty(apiKey))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }

                // 4. إرسال الطلب
                var response = await _httpClient.PostAsync(endpoint, content);

                // 5. معالجة الأخطاء (تحسين من الكود الجديد)
                // إذا فشل الطلب، نرجع تفاصيل الخطأ بدلاً من مجرد نص فارغ أو كراش
                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    return $"{{\"error\": \"API Error: {response.StatusCode}\", \"details\": \"{errorDetails.Replace("\"", "'")}\"}}";
                }

                // 6. إرجاع النتيجة (التي سيتم تحليلها في الـ Controller)
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // معالجة حالة فشل الاتصال بالسيرفر (Connection Failed)
                return $"{{\"error\": \"Connection failed: {ex.Message}\"}}";
            }
        }
    }
}