using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using TwitterWatcher.Models.Twitter;

namespace TwitterWatcher.Utility;

public static class HttpUtility
{
    private static readonly HttpClient _client = new();

    private const string _twitterBaseUri = "https://api.twitter.com";
    private static TwitterToken _twitterToken;

    private const string _telegramBaseUri = "https://api.telegram.org";
    private const int _telegramTimeBetweenMessages = 1000;
    private static readonly object _telegramLockObj = new();

    public static void TelegramPostRequest(string endpoint, Dictionary<string, string> urlParams = default)
    {
        _client.DefaultRequestHeaders.Authorization = null;

        string uri = QueryHelpers.AddQueryString($"{_telegramBaseUri}/bot{Config.Secrets.TelegramBotId}:{Config.Secrets.TelegramBotToken}{endpoint}", urlParams ?? new Dictionary<string, string>());

        HttpRequestMessage request = new(HttpMethod.Post, uri);

        lock (_telegramLockObj)
        {
            HttpResponseMessage response = Task.Run(async () => await _client.SendAsync(request)).Result;
            
            string content = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
            Log.Debug("Received response from Telegram: {Content}", content);

            Thread.Sleep(_telegramTimeBetweenMessages);
        }
    }
    
    public static async Task<T> TwitterGetRequest<T>(string endpoint, Dictionary<string, string> urlParams = default) where T : TwitterResponse
    {
        if (_twitterToken == null) _twitterToken = await GetTwitterBearerToken();
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _twitterToken.AccessToken);

        string uri = QueryHelpers.AddQueryString($"{_twitterBaseUri}{endpoint}", urlParams ?? new Dictionary<string, string>());
        
        HttpRequestMessage request = new(HttpMethod.Get, uri);
        HttpResponseMessage response = await _client.SendAsync(request);
        
        string content = Task.Run(async () => await response.Content.ReadAsStringAsync()).Result;
        Log.Debug("Received response from Twitter: {Content}", content);

        T result = JsonSerializer.Deserialize<T>(content);

        if (result?.Errors is { Length: > 0 })
        {
            Log.Error("An exception occurred while pulling from Twitter: {Errors}", result.Errors);
            return default;
        }

        return result;
    }

    private static async Task<TwitterToken> GetTwitterBearerToken()
    {
        _client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("UTF-8"));
        
        string consumerKey = System.Web.HttpUtility.UrlEncode(Config.Secrets.TwitterConsumerKey);
        string consumerSecret = System.Web.HttpUtility.UrlEncode(Config.Secrets.TwitterConsumerSecret);

        string authHeader = CommonUtility.ToBase64($"{consumerKey}:{consumerSecret}");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        HttpRequestMessage request = new(HttpMethod.Post, $"{_twitterBaseUri}/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "grant_type", "client_credentials" }
            })
        };

        HttpResponseMessage response = await _client.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<TwitterToken>();
    }
}