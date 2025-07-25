﻿using Apps.Contentful.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Errors;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace Apps.Contentful.Api;

public class ContentfulClient : ContentfulManagementClient
{
    private const int Limit = 100;
    private const int RetryCount = 5;

    private readonly AsyncRetryPolicy _retryPolicy;

    public ContentfulClient(IEnumerable<AuthenticationCredentialsProvider> creds, string? environment)
        : base(new HttpClient { Timeout=TimeSpan.FromMinutes(5)}, new ContentfulOptions
        {
            ManagementApiKey = creds.First(p => p.KeyName == "Authorization").Value,
            SpaceId = creds.First(p => p.KeyName == "spaceId").Value,
            Environment = environment,
            ManagementBaseUrl = creds.First(p => p.KeyName == CredNames.BaseUrl).Value + "/spaces/",
            MaxNumberOfRateLimitRetries = RetryCount,
            
        })
    {
        _retryPolicy = Policy
            .Handle<ContentfulRateLimitException>()
            .Or<ObjectDisposedException>()
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
            return await _retryPolicy.ExecuteAsync(func);
        }
        catch (ContentfulException ex)
        {
            throw new PluginApplicationException(ex.Message);
        }
        catch (JsonReaderException jex)
        {
            throw new PluginApplicationException("Error parsing JSON response: " + jex.Message);
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
            await func.Invoke();
        }
        catch (ContentfulException e)
        {
            throw new PluginApplicationException(e.Message);
        }
        catch (JsonReaderException jex)
        {
            throw new PluginApplicationException("Error parsing JSON response: " + jex.Message);
        }
    }
}