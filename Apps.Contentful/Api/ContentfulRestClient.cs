using System.Net;
using Apps.Contentful.Constants;
using Apps.Contentful.Models.Wrappers;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Apps.Contentful.Api;

public class ContentfulRestClient(AuthenticationCredentialsProvider[] creds, string? environment)
    : BlackBirdRestClient(new()
    {
        BaseUrl =
            $"{creds.Get(CredNames.BaseUrl).Value}/spaces/{creds.Get(CredNames.SpaceId).Value}{GetEnvironmentSegment(environment)}"
                .ToUri()
    })
{
    private const int RetryCount = 3;
    private const int WaitBeforeRetrySeconds = 3;

    private readonly AsyncRetryPolicy<RestResponse> _retryPolicy = Policy
        .HandleResult<RestResponse>(response => response.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(RetryCount, _ => TimeSpan.FromSeconds(WaitBeforeRetrySeconds));

    public async Task<IEnumerable<T>> Paginate<T>(ContentfulRestRequest request)
    {
        var result = new List<T>();
        var total = -1;
        while (result.Count != total)
        {
            request.AddOrUpdateParameter("skip", result.Count.ToString());
            var res = await ExecuteWithErrorHandling<ItemWrapper<T>>(request);
            total = res.Total;
            if (res.Items != null)
                result.AddRange(res.Items);
        }

        return result;
    }

    public override async Task<RestResponse> ExecuteWithErrorHandling(RestRequest request)
    {
        var response = await _retryPolicy.ExecuteAsync(() => ExecuteAsync(request));
        return response.IsSuccessStatusCode ? response : throw ConfigureErrorException(response);
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var error = JsonConvert.DeserializeObject<JObject>(response.Content);
        var details = error["details"];
        
        var errorMessages = new List<string>();

        if (details != null)
        {
            if (details.Type == JTokenType.Object && details["errors"] != null)
            {
                foreach (var errorItem in details["errors"])
                {
                    foreach (var property in errorItem.Children<JProperty>())
                    {
                        var field = property.Name;
                        var message = property.Value["message"]?.ToString();

                        if (!string.IsNullOrEmpty(message))
                        {
                            errorMessages.Add($"{field}: {message}");
                        }
                    }
                }
            }
            else if (details.Type == JTokenType.String)
            {
                var detailMessage = details.ToString();
                if (!string.IsNullOrEmpty(detailMessage))
                {
                    errorMessages.Add(detailMessage);
                }
            }
            else
            {
                errorMessages.Add($"Unexpected error details format: {details}");
            }
        }

        var fullMessage = error["message"]?.ToString() ?? error.ToString();
        var errors = string.Join("; ", errorMessages);
        return new PluginApplicationException($"{fullMessage} - {errors}");
    }

    private static string GetEnvironmentSegment(string? environment) =>
        string.IsNullOrWhiteSpace(environment) ? string.Empty : $"/environments/{environment}/";
}