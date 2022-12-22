using System.Text;

namespace TwitterWatcher.Utility;

public static class CommonUtility
{
    public static string ToBase64(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(bytes);
    }
    
    public static string FromBase64(string text)
    {
        byte[] bytes = Convert.FromBase64String(text);
        return Encoding.UTF8.GetString(bytes);
    }
}