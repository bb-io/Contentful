namespace Apps.Contentful.Utils;

public static class UrlHelper
{
    public static string FormatUrl(string baseUrl)
    {
        return !baseUrl.EndsWith("/") 
            ? baseUrl 
            : baseUrl.Substring(0, baseUrl.Length - 1);
    }
}