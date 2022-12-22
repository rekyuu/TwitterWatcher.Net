using System.Text.Json.Serialization;

namespace TwitterWatcher.Models.Twitter;

public class TwitterResponse
{
    [JsonPropertyName("errors")]
    public object[] Errors { get; set; }
}