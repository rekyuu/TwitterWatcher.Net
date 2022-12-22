using System.Text.Json.Serialization;

namespace TwitterWatcher.Models.Twitter;

public class TwitterUserTimeline : TwitterResponse
{
    [JsonPropertyName("data")]
    public Tweet[] Data { get; set; }

    [JsonPropertyName("includes")]
    public Includes Includes { get; set; }

    [JsonPropertyName("meta")]
    public Meta Meta { get; set; }
}

public class Tweet
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("attachments")]
    public Attachments Attachments { get; set; }
}

public class Attachments
{
    [JsonPropertyName("media_keys")]
    public string[] MediaKeys { get; set; }
}

public class Includes
{
    [JsonPropertyName("media")]
    public Media[] Media { get; set; }
}

public class Media
{
    [JsonPropertyName("media_key")]
    public string MediaKey { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("variants")]
    public Variant[] Variants { get; set; }
}

public class Variant
{
    [JsonPropertyName("bit_rate")]
    public long BitRate { get; set; }

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class Meta
{
    [JsonPropertyName("next_token")]
    public string NextToken { get; set; }

    [JsonPropertyName("result_count")]
    public int ResultCount { get; set; }

    [JsonPropertyName("newest_id")]
    public string NewestId { get; set; }

    [JsonPropertyName("oldest_id")]
    public string OldestId { get; set; }
}

