using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TwitterWatcher.Models;
using TwitterWatcher.Models.Telegram;
using TwitterWatcher.Models.Twitter;

namespace TwitterWatcher.Services;

public class WatcherService : IHostedService, IDisposable
{
    private readonly ILogger<WatcherService> _logger;
    private readonly DatabaseContext _db;
    
    private Timer _timer = null;

    public WatcherService(ILogger<WatcherService> logger)
    {
        _logger = logger;
        _db = new DatabaseContext(new DbContextOptions<DatabaseContext>());
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting watcher loop");
        _timer = new Timer(StartWatcher, null, TimeSpan.Zero, TimeSpan.FromMinutes(Config.WatcherCadenceMinutes));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping watcher loop");
        _timer.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    private void StartWatcher(object state)
    {
        Task.Run(async () => await Watch());
    }

    private async Task Watch()
    {
        Artist[] artists = _db.Artists
            // .Where(x => x.IsActive)
            .ToArray();
        
        _logger.LogInformation("Processing {Count} artists", artists.Length);

        int successful = 0;
        int failed = 0;
        
        foreach (Artist artist in artists)
        {
            try
            {
                _logger.LogInformation("Processing {Artist}...", artist.Username);
                
                TwitterUser user = await TwitterUtility.GetUserById(artist.TwitterId);
                if (user != null && user.Data.Username != artist.Username)
                {
                    _logger.LogInformation("{Artist}'s username has changed to {NewUsername}", artist.Username, user.Data.Username);
                    
                    List<string> usernameHistory = artist.UsernameHistory.ToList();
                    usernameHistory.Add(user.Data.Username);

                    artist.Username = user.Data.Username;
                    artist.UsernameHistory = usernameHistory.ToArray();
                }

                await artist.GetMediaTweets();

                if (artist.MediaTweets.Length == 0)
                {
                    successful++;
                    continue;
                }

                _logger.LogInformation("Sending {Count} media tweets...", artist.MediaTweets.Length);
                foreach (ArtistMediaTweet tweet in artist.MediaTweets)
                {
                    _logger.LogDebug("Sending {Count} media files for tweet...", tweet.Media.Length);
                    
                    if (tweet.Media.Length == 1) SendSingleFile(user, tweet);
                    else SendMultipleFiles(user, tweet);
                }

                if (!artist.IsActive) artist.IsActive = true;
                successful++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception was thrown while processing {Artist} and the user will be deactivated", artist.Username);
                TelegramUtility.SendMessage($"There was an issue processing https://twitter.com/{artist.Username}.");
                
                if (artist.IsActive) artist.IsActive = false;
                failed++;
            }
        }
                
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Finished processing {Count} artists. {Successful} successful, {Failed} failed", artists.Length, successful, failed);
    }

    private void SendSingleFile(TwitterUser user, ArtistMediaTweet tweet)
    {
        Media media = tweet.Media.First();

        string caption = CreateHtmlCaption(user, tweet);

        switch (media.Type)
        {
            case "photo":
                TelegramUtility.SendPhoto(caption, media.Url);
                break;
            case "video":
            case "animated_gif":
                string url = media.Variants
                    .OrderByDescending(x => x.BitRate)
                    .First()
                    .Url;
                
                TelegramUtility.SendVideo(caption, url);
                break;
            default:
                _logger.LogError("Media type is not valid:", media.Type);
                break;
        }
    }

    private static void SendMultipleFiles(TwitterUser user, ArtistMediaTweet tweet)
    {
        string caption = CreateHtmlCaption(user, tweet);
        
        TelegramInputMedia[] telegramMedia = tweet.Media.Select(x => 
            new TelegramInputMedia()
            {
                Type = x.Type,
                Media = x.Type switch
                {
                    "photo" => x.Url,
                    "video" => x.Variants
                        .OrderByDescending(v => v.BitRate)
                        .First()
                        .Url,
                    _ => ""
                }
            }
        ).ToArray();

        TelegramUtility.SendMediaGroup(telegramMedia);
        TelegramUtility.SendMessage(caption);
    }

    private static string CreateHtmlCaption(TwitterUser user, ArtistMediaTweet tweet)
    {
        string profile = $"https://twitter.com/{user.Data.Username}";
        string caption = TelegramUtility.EscapeTextForHtml(TrimEndingTcoLink(tweet.TweetText));
        string tweetUrl = $"{profile}/status/{tweet.TweetId}";
        
        return $"<b><a href=\"{profile}\">{user.Data.Name}</a></b> <a href=\"{tweetUrl}\">🔗</a>\n<i>@‎{user.Data.Username}</i>\n\n{caption}";
    }

    private static string CreateMarkdownCaption(TwitterUser user, ArtistMediaTweet tweet)
    {
        string profile = $"https://twitter.com/{user.Data.Username}";
        string caption = TelegramUtility.EscapeTextForMarkdown(TrimEndingTcoLink(tweet.TweetText));
        string tweetUrl = $"{profile}/status/{tweet.TweetId}";
        
        return $"*[{user.Data.Name}]({profile})* [🔗]({tweetUrl})\n_@‎{user.Data.Username}_\n\n{caption}";
    }

    private static string TrimEndingTcoLink(string input)
    {
        Regex rx = new Regex("^(?:.*)?(?<url>https?://t.co/[a-zA-Z0-9_]+)$", RegexOptions.Singleline);
        Match match = rx.Match(input);

        if (!match.Success) return input;

        return input.Replace($"{match.Groups["url"]}", "").Trim();
    }
}