namespace AMPFashionStore.Services
{
    public class AiSentimentResult
    {
        public bool Success { get; set; }
        public string Label { get; set; } = "normal"; // negative, normal, positive
        public double Confidence { get; set; }
        public Dictionary<string, double>? Probabilities { get; set; }
        public double? GateScore { get; set; }
        public bool IsNegative => Label.Equals("negative", StringComparison.OrdinalIgnoreCase);
        public string? ErrorMessage { get; set; }

        public string LabelDisplayName => Label?.ToLowerInvariant() switch
        {
            "negative" => "Tiêu cực",
            "positive" => "Tích cực",
            "normal" => "Bình thường",
            _ => "Không xác định"
        };
    }

    public class AiHealthResult
    {
        public bool IsOnline { get; set; }
        public string Status { get; set; } = "offline";
        public string Device { get; set; } = "unknown";
        public string ModelName { get; set; } = "PhoBERT-AMP";
        public List<string> Labels { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class AiCommentBatchItem
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool HasImage { get; set; }
    }

    public interface IAiSentimentService
    {
        Task<AiSentimentResult> AnalyzeSentimentAsync(string text, int? rating = null, bool hasImage = false);
        Task<Dictionary<int, AiSentimentResult>> AnalyzeBatchSentimentAsync(IEnumerable<AiCommentBatchItem> items);
        Task<AiHealthResult> CheckHealthAsync();
    }
}
