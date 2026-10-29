using Apps.Contentful.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Contentful.Core;
using Contentful.Core.Configuration;

namespace Apps.Contentful.Api;

public class ContentfulClient : ContentfulManagementClient
{
    private const int Limit = 100;

    public ContentfulClient(IEnumerable<AuthenticationCredentialsProvider> creds, string? environment)
        : base(new HttpClient(ContentfulHttpHandlers.Wrap(new HttpClientHandler())) { Timeout = TimeSpan.FromMinutes(5) }, new ContentfulOptions
        {
            ManagementApiKey = creds.First(p => p.KeyName == "Authorization").Value,
            SpaceId = creds.First(p => p.KeyName == "spaceId").Value,
            Environment = environment,
            ManagementBaseUrl = creds.First(p => p.KeyName == CredNames.BaseUrl).Value + "/spaces/",
            MaxNumberOfRateLimitRetries = 0,
        })
    {
    }

    public string GetEntryEditorUrl(string entryId)
    {
        if (_options.Environment is not null)
        {
            return $"https://app.contentful.com/spaces/{_options.SpaceId}/environments/{_options.Environment}/entries/{entryId}";
        } else
        {
            return $"https://app.contentful.com/spaces/{_options.SpaceId}/entries/{entryId}";
        }
    }

    public async Task<IEnumerable<T>> Paginate<T>(Func<string, Task<IEnumerable<T>>> method, string? initialQueryString)
    {
        var result = new List<T>();

        while (true)
        {
            var query = string.IsNullOrEmpty(initialQueryString) ? "?" : initialQueryString;
            var items = await method(query + $"&skip={result.Count}&limit={Limit}");
            result.AddRange(items);
            if (items.Count() < Limit)
                break;
        }

        return result;
    }
}
