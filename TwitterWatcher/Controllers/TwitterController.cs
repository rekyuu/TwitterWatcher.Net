using Microsoft.AspNetCore.Mvc;
using TwitterWatcher.Models;
using TwitterWatcher.Models.Twitter;

namespace TwitterWatcher.Controllers;

[ApiController]
[RequiresAuthHeader]
[Route("api/v1/twitter")]
public class TwitterController : ControllerBase
{
    private readonly ILogger<TwitterController> _logger;

    public TwitterController(ILogger<TwitterController> logger)
    {
        _logger = logger;

    }

    [HttpGet("user-id/{username}")]
    public async Task<TwitterUser> GetUserId(string username)
    {
        return await TwitterUtility.GetUserByUsername(username);
    }

    [HttpGet("tweets/{id}")]
    public async Task<TwitterUserTimeline> GetTweets(string id, string sinceId = "")
    {
        return await TwitterUtility.GetUserTimeline(id, sinceId);
    }

    [HttpGet("media-tweets/{id}")]
    public async Task<ArtistMediaTweet[]> GetMediaTweets(string id, string sinceId = "")
    {
        return await TwitterUtility.GetUserMediaTweets(id, sinceId);
    }
}