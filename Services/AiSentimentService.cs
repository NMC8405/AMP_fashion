using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AMPFashionStore.Services
{
    public class AiSentimentService : IAiSentimentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiSentimentService> _logger;
        private readonly string _baseUrl;

        public AiSentimentService(HttpClient httpClient, IConfiguration configuration, ILogger<AiSentimentService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = (configuration["AiService:Url"] ?? "http://127.0.0.1:8000").TrimEnd('/');
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        public async Task<AiHealthResult> CheckHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
                if (!response.IsSuccessStatusCode)
                {
                    return new AiHealthResult
                    {
                        IsOnline = false,
                        Status = $"HTTP {response.StatusCode}",
                        ErrorMessage = $"Service trả về mã lỗi HTTP {response.StatusCode}"
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                var health = JsonSerializer.Deserialize<FastApiHealthResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return new AiHealthResult
                {
                    IsOnline = true,
                    Status = health?.Status ?? "ok",
                    Device = health?.Device ?? "cpu",
                    ModelName = health?.PhobertModelName ?? "PhoBERT-AMP (Adaptive Metadata Gate)",
                    Labels = health?.Labels ?? new List<string> { "negative", "normal", "positive" }
                };
            }
            catch (Exception ex)
            {
                return new AiHealthResult
                {
                    IsOnline = false,
                    Status = "offline",
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<AiSentimentResult> AnalyzeSentimentAsync(string text, int? rating = null, bool hasImage = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new AiSentimentResult
                {
                    Success = true,
                    Label = "normal",
                    Confidence = 1.0
                };
            }

            var cleanText = text.Trim();

            try
            {
                // Chuẩn payload tương thích 100% với model_phanloaibinhluantiengviet_AMP_DCT-main (api/schemas.py)
                var requestBody = new
                {
                    text = cleanText,
                    comment_length = cleanText.Length,
                    word_count = cleanText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length,
                    hasImage = hasImage ? 1 : 0
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_baseUrl}/predict", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("PhoBERT AI Service HTTP {StatusCode}: {Error}", response.StatusCode, errorText);
                    return new AiSentimentResult
                    {
                        Success = false,
                        Label = "normal",
                        Confidence = 0.0,
                        ErrorMessage = $"HTTP {response.StatusCode}: {errorText}"
                    };
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<FastApiPredictResponse>(responseJson, options);

                if (result == null)
                {
                    return new AiSentimentResult
                    {
                        Success = false,
                        Label = "normal",
                        ErrorMessage = "Phản hồi từ PhoBERT AI rỗng"
                    };
                }

                return new AiSentimentResult
                {
                    Success = true,
                    Label = result.Label?.ToLowerInvariant() ?? "normal",
                    Confidence = result.Confidence,
                    Probabilities = result.Probabilities,
                    GateScore = result.GateScore
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể kết nối đến PhoBERT-AMP Service tại {BaseUrl}. Sử dụng chế độ dự phòng an toàn.", _baseUrl);
                return new AiSentimentResult
                {
                    Success = false,
                    Label = "normal",
                    Confidence = 0.0,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<Dictionary<int, AiSentimentResult>> AnalyzeBatchSentimentAsync(IEnumerable<AiCommentBatchItem> items)
        {
            var dict = new Dictionary<int, AiSentimentResult>();
            var itemList = items.ToList();

            if (!itemList.Any())
            {
                return dict;
            }

            try
            {
                // Chuẩn payload Batch tương thích model_phanloaibinhluantiengviet_AMP_DCT-main
                var requestBody = new
                {
                    comments = itemList.Select(i =>
                    {
                        var t = (i.Text ?? "").Trim();
                        return new
                        {
                            text = t,
                            comment_length = t.Length,
                            word_count = t.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length,
                            hasImage = i.HasImage ? 1 : 0
                        };
                    }).ToList()
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_baseUrl}/predict/batch", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var batchResult = JsonSerializer.Deserialize<FastApiBatchPredictResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (batchResult?.Results != null && batchResult.Results.Count == itemList.Count)
                    {
                        for (int i = 0; i < itemList.Count; i++)
                        {
                            var r = batchResult.Results[i];
                            dict[itemList[i].Id] = new AiSentimentResult
                            {
                                Success = true,
                                Label = r.Label?.ToLowerInvariant() ?? "normal",
                                Confidence = r.Confidence,
                                Probabilities = r.Probabilities,
                                GateScore = r.GateScore
                            };
                        }
                        return dict;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Batch predict thất bại, chuyển sang chế độ phân tích tuần tự.");
            }

            // Chế độ dự phòng tuần tự nếu Batch gặp lỗi mạng
            foreach (var item in itemList)
            {
                dict[item.Id] = await AnalyzeSentimentAsync(item.Text, null, item.HasImage);
            }

            return dict;
        }

        private class FastApiPredictResponse
        {
            public string? Text { get; set; }
            public string? Label { get; set; }
            public double Confidence { get; set; }
            public Dictionary<string, double>? Probabilities { get; set; }

            [JsonPropertyName("gate_score")]
            public double? GateScore { get; set; }
        }

        private class FastApiBatchPredictResponse
        {
            public List<FastApiPredictResponse>? Results { get; set; }
        }

        private class FastApiHealthResponse
        {
            public string? Status { get; set; }
            public string? Device { get; set; }

            [JsonPropertyName("phobert_model_name")]
            public string? PhobertModelName { get; set; }
            public List<string>? Labels { get; set; }
        }
    }
}
