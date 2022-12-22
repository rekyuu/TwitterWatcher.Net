using System.Text.Json.Serialization;

namespace TwitterWatcher.Models.Telegram;

public class TelegramInputMedia
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("media")]
    public string Media { get; set; }
}