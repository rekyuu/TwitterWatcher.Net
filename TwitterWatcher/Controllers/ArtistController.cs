using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using TwitterWatcher.Models;
using TwitterWatcher.Models.Twitter;

namespace TwitterWatcher.Controllers;

[ApiController]
[RequiresAuthHeader]
[Route("api/v1/artists")]
public class ArtistController : ControllerBase
{
    private readonly DatabaseContext _db;
    private readonly ILogger<ArtistController> _logger;

    public ArtistController(ILogger<ArtistController> logger, DatabaseContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<Artist[]>> GetAllArtists(bool media = false)
    {
        if (media)
        {
            foreach (Artist artist in _db.Artists) await artist.GetMediaTweets();
        }
        
        return Ok(_db.Artists);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Artist>> GetArtist(string id, bool media = false)
    {
        Artist artist = await _db.Artists.FindAsync(id);

        if (artist == null) return NotFound();
        if (media) await artist.GetMediaTweets();
        
        return Ok(artist);
    }

    [HttpPost]
    public async Task<ActionResult<Artist>> AddArtist(string twitterUrl)
    {
        Regex rx = new Regex("https?://(?:www.)?(?:[v|f]x)?twitter.com/(?<username>[a-zA-Z0-9_]+)(?:/.*)?");
        Match match = rx.Match(twitterUrl);

        if (!match.Success) return BadRequest("Unable to parse username form the provided Twitter URL.");

        string username = match.Groups["username"].Value;
        TwitterUser user = await TwitterUtility.GetUserByUsername(username);
        Artist artist = await _db.Artists.FindAsync(user.Data.Id);

        if (artist != null) return BadRequest("Artist ID already exists.");
        
        artist = await CreateArtistModel(user);

        await _db.Artists.AddAsync(artist);
        await _db.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetArtist), new { id = artist.TwitterId }, artist);
    }

    [HttpPost("batch")]
    public async Task<ActionResult<object>> AddBatchArtists([FromBody] string[] usernames)
    {
        List<Artist> created = new();
        List<string> failed = new();
        List<string> alreadyExists = new();
        
        foreach (string username in usernames)
        {
            try
            {
                TwitterUser user = await TwitterUtility.GetUserByUsername(username);
                Artist artist = await _db.Artists.FindAsync(user.Data.Id);

                if (artist != null)
                {
                    alreadyExists.Add(username);
                    continue;
                }
                
                artist = await CreateArtistModel(user);

                created.Add(artist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Failed to create user: {Username}", username);
                failed.Add(username);
            }
        }

        await _db.Artists.AddRangeAsync(created);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllArtists), null, new
        {
            created,
            failed,
            alreadyExists
        });
    }
    

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteArtist(string id)
    {
        Artist artist = await _db.Artists.FindAsync(id);

        if (artist == null) return NotFound();

        _db.Artists.Remove(artist);
        await _db.SaveChangesAsync();

        return Ok();
    }

    private static async Task<Artist> CreateArtistModel(TwitterUser user)
    {
        Artist artist = new()
        {
            TwitterId = user.Data.Id,
            Username = user.Data.Username,
            UsernameHistory = new [] { user.Data.Username },
            IsActive = true
        };

        await artist.GetMediaTweets();

        return artist;
    }
}