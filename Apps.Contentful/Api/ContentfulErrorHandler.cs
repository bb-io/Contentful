using System.Net;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.Api;

public class ContentfulErrorHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new PluginApplicationException(
                $"Connection error while communicating with Contentful: {exception.Message}", exception);
        }

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotModified)
        {
            return response;
        }

        using (response)
        {
            throw await CreateException(response, cancellationToken);
        }
    }

    private static async Task<PluginApplicationException> CreateException(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryMessage = ContentfulPollyPolicies.TryGetServerDelay(response, out var retryDelay)
                ? $" Contentful requested waiting {Math.Ceiling(retryDelay.TotalSeconds)} seconds before the next request."
                : string.Empty;

            return new PluginApplicationException(
                $"Contentful rate limit was exceeded.{retryMessage} Please try again later or add a retry step to the Bird.");
        }

        var error = TryParseJsonObject(await response.Content.ReadAsStringAsync(cancellationToken));
        var message = error?["message"]?.ToString();
        if (string.IsNullOrWhiteSpace(message))
        {
            message = GetGenericMessage(response);
        }

        var errorDetails = (error?["details"] as JObject)?["errors"]?.ToString();

        return new PluginApplicationException(message + errorDetails);
    }

    private static string GetGenericMessage(HttpResponseMessage response) => response.StatusCode switch
    {
        HttpStatusCode.BadRequest => "The request contained invalid or unknown query parameters.",
        HttpStatusCode.Unauthorized => "The authorization token was invalid.",
        HttpStatusCode.Forbidden => "The specified token does not have access to the requested resource.",
        HttpStatusCode.NotFound => "The requested resource or endpoint could not be found.",
        HttpStatusCode.Conflict =>
            "Version mismatch error. The version you specified was incorrect. This may be due to someone else editing the content.",
        HttpStatusCode.UnprocessableEntity => "Validation failed. The request references an invalid field.",
        HttpStatusCode.InternalServerError => "Internal server error.",
        HttpStatusCode.BadGateway => "The requested space is hibernated.",
        HttpStatusCode.GatewayTimeout => "Gateway timeout.",
        _ => $"Contentful returned error {(int)response.StatusCode} {response.ReasonPhrase}."
    };

    private static JObject? TryParseJsonObject(string content)
    {
        try
        {
            return JObject.Parse(content);
        }
        catch (JsonReaderException)
        {
            return null;
        }
    }
}
