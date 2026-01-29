using RestSharp;

namespace Apps.Contentful.Utils;

public class WebhookLogger
{
    private const string BaseUrl = "https://webhook.site/099dc0b3-1dcf-47b6-804d-46758ebf967f";
    
    public static async Task LogAsync<T>(T obj)
        where T : class
    {
        var restClient = new RestClient(BaseUrl);
        var request = new RestRequest(String.Empty, Method.Post)
            .AddJsonBody(obj);
        
        await restClient.ExecuteAsync(request);
    }
}