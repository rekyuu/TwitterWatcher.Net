using System.Text.Json;
using TwitterWatcher.Models.Telegram;
using TwitterWatcher.Utility;

namespace TwitterWatcher;

public static class TelegramUtility
{
    private static readonly char[] _telegramMarkdownEscapeChars = 
    {
        '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!'
    };

    public static void SendMessage(string text)
    {
        Dictionary<string, string> urlParams = new()
        {
            { "chat_id", Config.Secrets.TelegramChatId },
            { "parse_mode", "HTML" },
            { "disable_web_page_preview", "true" },
            { "text", text }
        };

        HttpUtility.TelegramPostRequest("/sendMessage", urlParams);
    }

    public static void SendPhoto(string caption, string photoUrl)
    {
        Dictionary<string, string> urlParams = new()
        {
            { "chat_id", Config.Secrets.TelegramChatId },
            { "parse_mode", "HTML" },
            { "caption", caption },
            { "photo", photoUrl }
        };

        HttpUtility.TelegramPostRequest("/sendPhoto", urlParams);
    }

    public static void SendVideo(string caption, string videoUrl)
    {
        Dictionary<string, string> urlParams = new()
        {
            { "chat_id", Config.Secrets.TelegramChatId },
            { "parse_mode", "HTML" },
            { "caption", caption },
            { "video", videoUrl }
        };

        HttpUtility.TelegramPostRequest("/sendVideo", urlParams);
    }

    public static void SendMediaGroup(TelegramInputMedia[] media)
    {
        Dictionary<string, string> urlParams = new()
        {
            { "chat_id", Config.Secrets.TelegramChatId },
            { "media", JsonSerializer.Serialize(media) }
        };

        HttpUtility.TelegramPostRequest("/sendMediaGroup", urlParams);
    }

    public static string EscapeTextForMarkdown(string input)
    {
        foreach (char character in _telegramMarkdownEscapeChars)
        {
            input = input.Replace($"{character}", $"\\{character}");
        }

        return input
            .Replace("#", "#‎")
            .Replace("@", "@‎");
    }

    public static string EscapeTextForHtml(string input)
    {
        return input
            .Replace("#", "#‎")
            .Replace("@", "@‎")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("&", "&amp;");
    }
}