using Apps.Contentful.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Errors;
using Polly;
using Polly.Retry;

namespace Apps.Contentful.Api;

public class ContentfulClient : ContentfulManagementClient
{
    private const int Limit = 100;
    private const int RetryCount = 5;
    
    private readonly AsyncRetryPolicy _retryPolicy;

    public ContentfulClient(IEnumerable<AuthenticationCredentialsProvider> creds, string? environment)
        : base(new HttpClient(), new ContentfulOptions
        {
            ManagementApiKey = creds.First(p => p.KeyName == "Authorization").Value,
            SpaceId = creds.First(p => p.KeyName == "spaceId").Value,
            Environment = environment,
            ManagementBaseUrl = creds.First(p => p.KeyName == CredNames.BaseUrl).Value + "/spaces/",
            MaxNumberOfRateLimitRetries = RetryCount
        })
    {
        _retryPolicy = Policy
            .Handle<ContentfulRateLimitException>()
            .WaitAndRetryAsync(RetryCount, (_) => TimeSpan.Zero, (exception, _) =>
            {
                if (exception is ContentfulRateLimitException contentfulRateLimitException)
                {
                    return Task.Delay(TimeSpan.FromSeconds(contentfulRateLimitException.SecondsUntilNextRequest + 5));
                }

                if (exception.Message.Contains("Version mismatch error"))
                {
                    return Task.Delay(TimeSpan.FromSeconds(5));
                }

                return Task.CompletedTask;
            });
    }

    public async Task<IEnumerable<T>> Paginate<T>(Func<string, Task<IEnumerable<T>>> method, string? initialQueryString)
    {
        var result = new List<T>();
        
        while(true)
        {
            var query = string.IsNullOrEmpty(initialQueryString) ? "?" : initialQueryString;
            var items = await ExecuteWithErrorHandling(() => method(query + $"&skip={result.Count}&limit={Limit}"));
            result.AddRange(items);
            if (items.Count() < Limit)
                break;
        }

        return result;        
    }

    public Task<T> ExecuteWithErrorHandling<T>(Func<Task<T>> func)
    {
        return _retryPolicy.ExecuteAsync(func);
    }
}