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

        public async Task<string> GetRawPredictionAsync(object payload)
        {
            var endpoint = _configuration["AiSettings:Endpoint"] ?? "http://127.0.0.1:5000/predict";
            var apiKey = _configuration["AiSettings:ApiKey"] ?? "";

            try
            {
                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(apiKey))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                }

                var response = await _httpClient.PostAsync(endpoint, content);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"Connection failed: {ex.Message}\"}}";
            }
        }
    }
}