using Apps.Contentful.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Errors;
using Newtonsoft.Json;
using Polly;

namespace Apps.Contentful.Api;

public class ContentfulClient : ContentfulManagementClient
{
    private const int Limit = 100;
    private static readonly ResiliencePipeline RetryPipeline = ContentfulPollyPolicies.CreateSdkPipeline();

    public ContentfulClient(IEnumerable<AuthenticationCredentialsProvider> creds, string? environment)
        : base(new HttpClient { Timeout = TimeSpan.FromMinutes(5) }, new ContentfulOptions
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
            var items = await ExecuteWithErrorHandling(() => method(query + $"&skip={result.Count}&limit={Limit}"));
            result.AddRange(items);
            if (items.Count() < Limit)
                break;
        }

        return result;
    }

    public async Task<T> ExecuteWithErrorHandling<T>(Func<Task<T>> func)
    {
        try
        {
            return await RetryPipeline.ExecuteAsync(
                async _ => await func(),
                CancellationToken.None);
        }
        catch (ContentfulRateLimitException ex)
        {
            throw CreateRateLimitException(ex);
        }
        catch (ContentfulException ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
        catch (JsonReaderException jex)
        {
            throw new PluginApplicationException("Error parsing JSON response: " + jex.Message);
        }
        catch (HttpRequestException ex)
        {
            throw new PluginApplicationException(
                $"Connection error while communicating with Contentful: {ex.Message}", ex);
        }
        catch (ObjectDisposedException ex) when (ex.ObjectName?.Contains("StreamContent") == true)
        {
            throw new PluginApplicationException("Connection error while communicating with Contentful. Please try again and add retries to this action.");
        }
    }

    public async Task ExecuteWithErrorHandling(Func<Task> func)
    {
        try
        {
            await RetryPipeline.ExecuteAsync(
                async _ => await func(),
                CancellationToken.None);
        }
        catch (ContentfulRateLimitException ex)
        {
            throw CreateRateLimitException(ex);
        }
        catch (ContentfulException e)
        {
            throw new PluginApplicationException(e.Message);
        }
        catch (JsonReaderException jex)
        {
            throw new PluginApplicationException("Error parsing JSON response: " + jex.Message);
        }
        catch (HttpRequestException ex)
        {
            throw new PluginApplicationException(
                $"Connection error while communicating with Contentful: {ex.Message}", ex);
        }
        catch (ObjectDisposedException ex) when (ex.ObjectName?.Contains("StreamContent") == true)
        {
            throw new PluginApplicationException("Connection error while communicating with Contentful. Please try again and add retries to this action.");
        }
    }

    private static PluginApplicationException CreateRateLimitException(ContentfulRateLimitException exception)
    {
        var retryMessage = exception.SecondsUntilNextRequest > 0
            ? $" Contentful requested waiting {exception.SecondsUntilNextRequest} seconds before the next request."
            : string.Empty;

        return new PluginApplicationException(
            $"Contentful rate limit was exceeded.{retryMessage} Please try again later or add a retry step to the Bird.",
            exception);
    }
}
