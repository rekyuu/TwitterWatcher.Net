using System.Text.Json.Serialization;

namespace TwitterWatcher.Models.Twitter;

public class TwitterToken
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
    
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}