using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Serilog;
using TwitterWatcher.Models.Twitter;

namespace TwitterWatcher.Models;

public class Artist
{
    [Key]
    [JsonPropertyName("twitter_id")]
    public string TwitterId { get; set; }
    
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("last_processed_tweet_id")]
    public string LastProcessedTweetId { get; set; }
    
    [JsonPropertyName("last_processed_time")]
    public DateTimeOffset LastProcessedTime { get; set; }
    
    [JsonPropertyName("last_tweet_time")]
    public DateTimeOffset LastTweetTime { get; set; }
    
    [JsonPropertyName("last_media_tweet_time")]
    public DateTimeOffset LastMediaTweetTime { get; set; }
    
    [JsonPropertyName("username_history")]
    public string[] UsernameHistory { get; set; }
    
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("media_tweets")]
    public ArtistMediaTweet[] MediaTweets { get; private set; } = Array.Empty<ArtistMediaTweet>();

    public async Task GetMediaTweets()
    {
        try
        {
            MediaTweets = Array.Empty<ArtistMediaTweet>();
            LastProcessedTime = DateTimeOffset.UtcNow;
            
            TwitterUserTimeline timeline = await TwitterUtility.GetUserTimeline(TwitterId, LastProcessedTweetId);
            if (timeline.Data == null || timeline.Data.Length == 0) return;
        
            LastTweetTime = timeline.Data.Last().CreatedAt;
            LastProcessedTweetId = timeline.Data.Last().Id;

            MediaTweets = TwitterUtility.GetUserMediaTweets(timeline);
            if (MediaTweets.Length == 0) return;
        
            LastMediaTweetTime = MediaTweets.Last().CreatedAt;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An exception was thrown while processing {Artist} and the user will be deactivated", Username);
            TelegramUtility.SendMessage($"There was an issue processing https://twitter.com/{Username}.");
                
            IsActive = false;
        }
    }
}

public class ArtistMediaTweet
{
    [JsonPropertyName("tweet_id")]
    public string TweetId { get; set; }
    
    [JsonPropertyName("tweet_text")]
    public string TweetText { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    
    [JsonPropertyName("media")]
    public Media[] Media { get; set; }
}