using Blackbird.Applications.Sdk.Common.Authentication;
using Contentful.Core;
using Contentful.Core.Configuration;

namespace Apps.Contentful.Api;

public class ContentfulClient : ContentfulManagementClient
{
    private const int limit = 100;

    public ContentfulClient(IEnumerable<AuthenticationCredentialsProvider> creds, string? environment)
        : base(new HttpClient(), new ContentfulOptions
        {
            ManagementApiKey = creds.First(p => p.KeyName == "Authorization").Value,
            SpaceId = creds.First(p => p.KeyName == "spaceId").Value,
            Environment = environment
        })
    {
    }

    public async Task<IEnumerable<T>> Paginate<T>(Func<string, Task<IEnumerable<T>>> method, string? initialQueryString)
    {
        var result = new List<T>();
        
        while(true)
        {
            var query = string.IsNullOrEmpty(initialQueryString) ? "?" : initialQueryString;
            var items = await method(query + $"&skip={result.Count}&limit={limit}");
            result.AddRange(items);
            if (items.Count() < limit)
                break;
        }

        return result;        
    }
}