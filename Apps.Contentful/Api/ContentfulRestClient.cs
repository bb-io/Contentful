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
using RestSharp;

namespace Apps.Contentful.Api;

public class ContentfulRestClient(AuthenticationCredentialsProvider[] creds, string? environment)
    : BlackBirdRestClient(new()
    {
        BaseUrl =
            $"{creds.Get(CredNames.BaseUrl).Value}/spaces/{creds.Get(CredNames.SpaceId).Value}{GetEnvironmentSegment(environment)}"
                .ToUri(),
        MaxTimeout=180000
    })
{
    private static readonly ResiliencePipeline<RestResponse> RetryPipeline =
        ContentfulPollyPolicies.CreateRestPipeline();

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
        RestResponse response;
        try
        {
            response = await RetryPipeline.ExecuteAsync(
                cancellationToken => new ValueTask<RestResponse>(ExecuteAsync(request, cancellationToken)));
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new PluginApplicationException(
                "Contentful rate limit was exceeded. Please try again later or add a retry step to the Bird.",
                exception);
        }
        catch (HttpRequestException exception) when (exception.StatusCode is
                                                     HttpStatusCode.InternalServerError or
                                                     HttpStatusCode.ServiceUnavailable)
        {
            throw new PluginApplicationException(
                "Contentful is temporarily unavailable after multiple retry attempts. Please try again later.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new PluginApplicationException(
                $"Connection error while communicating with Contentful: {exception.Message}",
                exception);
        }

        return response.IsSuccessStatusCode ? response : throw ConfigureErrorException(response);
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryMessage = ContentfulPollyPolicies.TryGetServerDelay(response, out var retryDelay)
                ? $" Contentful requested waiting {Math.Ceiling(retryDelay.TotalSeconds)} seconds before the next request."
                : string.Empty;

            return new PluginApplicationException(
                $"Contentful rate limit was exceeded.{retryMessage} Please try again later or add a retry step to the Bird.");
        }

        JObject? error = null;
        if (!string.IsNullOrWhiteSpace(response.Content))
        {
            try
            {
                error = JsonConvert.DeserializeObject<JObject>(response.Content);
            }
            catch (JsonException)
            {
                // Preserve the original HTTP error when Contentful returns a non-JSON response.
            }
        }

        if (error is null)
        {
            var statusDescription = string.IsNullOrWhiteSpace(response.StatusDescription)
                ? response.StatusCode.ToString()
                : response.StatusDescription;
            return new PluginApplicationException(
                $"Contentful returned error {(int)response.StatusCode} {statusDescription}. {response.Content}".Trim());
        }

        var details = error["details"];
        
        var errorMessages = new List<string>();

        if (details != null)
        {
            var detailErrors = details["errors"];
            if (details.Type == JTokenType.Object && detailErrors != null)
            {
                foreach (var errorItem in detailErrors)
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
