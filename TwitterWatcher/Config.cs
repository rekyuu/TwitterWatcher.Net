using System.Text.Json;
using System.Text.Json.Serialization;

namespace TwitterWatcher;

public static class Config
{
    public static Secrets Secrets { get; }
    
    static Config()
    {
        Secrets = new Secrets();
        
        if (File.Exists("Secrets.json"))
        {
            string secrets = File.ReadAllText("Secrets.json");
            Secrets = JsonSerializer.Deserialize<Secrets>(secrets);
        }
    }
    
    public static string PsqlConnectionString => $"Host={Secrets.PsqlHost};Database={Secrets.PsqlDb};Username={Secrets.PsqlUsername};Password={Secrets.PsqlPassword}";
}

public class Secrets
{
    [JsonPropertyName("psql_host")]
    public string PsqlHost { get; set; } = Environment.GetEnvironmentVariable("PSQL_HOST");
    
    [JsonPropertyName("psql_db")]
    public string PsqlDb { get; set; } = Environment.GetEnvironmentVariable("PSQL_DB");
    
    [JsonPropertyName("psql_username")]
    public string PsqlUsername { get; set; } = Environment.GetEnvironmentVariable("PSQL_USERNAME");
    
    [JsonPropertyName("psql_password")]
    public string PsqlPassword { get; set; } = Environment.GetEnvironmentVariable("PSQL_PASSWORD");
    
    [JsonPropertyName("telegram_bot_id")]
    public string TelegramBotId { get; set; } = Environment.GetEnvironmentVariable("TELEGRAM_BOT_ID");

    [JsonPropertyName("telegram_bot_token")]
    public string TelegramBotToken { get; set; } = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

    [JsonPropertyName("telegram_chat_id")]
    public string TelegramChatId { get; set; } = Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID");
    
    [JsonPropertyName("twitter_consumer_key")]
    public string TwitterConsumerKey { get; set; } = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY");

    [JsonPropertyName("twitter_consumer_secret")]
    public string TwitterConsumerSecret { get; set; } = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_SECRET");

    [JsonPropertyName("twitter_access_token")]
    public string TwitterAccessToken { get; set; } = Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN");

    [JsonPropertyName("twitter_access_secret")]
    public string TwitterAccessSecret { get; set; } = Environment.GetEnvironmentVariable("TWITTER_ACCESS_SECRET");

    [JsonPropertyName("x_access_token")]
    public string XAccessToken { get; set; } = Environment.GetEnvironmentVariable("X_ACCESS_TOKEN");
}