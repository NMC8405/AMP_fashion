using System.Text.Json;

namespace AMPFashionStore.Services
{
    /// <summary>Tiện ích lưu/đọc object (JSON) trong Session - dùng System.Text.Json có sẵn, không cần NuGet.</summary>
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
