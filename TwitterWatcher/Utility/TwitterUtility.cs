using TwitterWatcher.Models;
using TwitterWatcher.Models.Twitter;
using TwitterWatcher.Utility;

namespace TwitterWatcher;

public static class TwitterUtility
{
    public static async Task<TwitterUser> GetUserByUsername(string username)
    {
        return await HttpUtility.TwitterGetRequest<TwitterUser>($"/2/users/by/username/{username}");
    }
    
    public static async Task<TwitterUser> GetUserById(string id)
    {
        return await HttpUtility.TwitterGetRequest<TwitterUser>($"/2/users/{id}");
    }

    public static async Task<TwitterUserTimeline> GetUserTimeline(string twitterId, string sinceId = "")
    {
        Dictionary<string, string> urlParams = new()
        {
            { "max_results", "100" },
            { "exclude", "retweets" },
            { "tweet.fields", "created_at" },
            { "expansions", "attachments.media_keys" },
            { "media.fields", "type,url,variants" }
        };
        
        if (!string.IsNullOrEmpty(sinceId)) urlParams.Add("since_id", sinceId);
        
        TwitterUserTimeline timeline = await HttpUtility.TwitterGetRequest<TwitterUserTimeline>($"/2/users/{twitterId}/tweets", urlParams);
        timeline.Data = timeline.Data?.OrderBy(x => x.CreatedAt).ToArray();
        
        return timeline;
    }

    public static async Task<ArtistMediaTweet[]> GetUserMediaTweets(string twitterId, string sinceId = "")
    {
        TwitterUserTimeline timeline = await GetUserTimeline(twitterId, sinceId);
        return GetUserMediaTweets(timeline);
    }

    public static ArtistMediaTweet[] GetUserMediaTweets(TwitterUserTimeline timeline)
    {
        if (timeline?.Data == null) return Array.Empty<ArtistMediaTweet>();
        
        List<ArtistMediaTweet> mediaTweets = new();
        foreach (Tweet tweet in timeline.Data)
        {
            if (tweet.Attachments == null) continue;

            ArtistMediaTweet mediaTweet = new()
            {
                TweetId = tweet.Id,
                TweetText = tweet.Text,
                CreatedAt = tweet.CreatedAt,
                Media = tweet.Attachments.MediaKeys
                    .Select(mediaKey => timeline.Includes.Media.First(x => x.MediaKey == mediaKey))
                    .ToArray()
            };
            
            mediaTweets.Add(mediaTweet);
        }

        return mediaTweets.OrderBy(x => x.CreatedAt).ToArray();
    }
}