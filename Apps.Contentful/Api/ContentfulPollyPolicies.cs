using System.Globalization;
using System.Net;
using Contentful.Core.Errors;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Apps.Contentful.Api;

public static class ContentfulPollyPolicies
{
    internal const int DefaultRetryCount = 6;
    internal static readonly TimeSpan MaximumServerDelay = TimeSpan.FromSeconds(60);

    private static readonly TimeSpan MaximumFallbackDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan MaximumJitter = TimeSpan.FromMilliseconds(500);

    public static ResiliencePipeline<RestResponse> CreateRestPipeline()
    {
        var options = new RetryStrategyOptions<RestResponse>
        {
            MaxRetryAttempts = DefaultRetryCount,
            ShouldHandle = new PredicateBuilder<RestResponse>()
                .HandleResult(ShouldRetryRestResponse)
                .Handle<HttpRequestException>(ShouldRetryHttpRequestException),
            DelayGenerator = args => new ValueTask<TimeSpan?>(
                GetDelay(args.Outcome.Result, args.Outcome.Exception, args.AttemptNumber))
        };

        return new ResiliencePipelineBuilder<RestResponse>()
            .AddRetry(options)
            .Build();
    }

    public static ResiliencePipeline CreateSdkPipeline()
    {
        var options = new RetryStrategyOptions
        {
            MaxRetryAttempts = DefaultRetryCount,
            ShouldHandle = new PredicateBuilder()
                .Handle<ContentfulRateLimitException>(exception =>
                    IsAcceptableServerDelay(TimeSpan.FromSeconds(exception.SecondsUntilNextRequest)))
                .Handle<ContentfulException>(exception =>
                    exception is not ContentfulRateLimitException &&
                    exception.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase))
                .Handle<ObjectDisposedException>()
                .Handle<HttpRequestException>(ShouldRetryHttpRequestException)
                .Handle<Exception>(exception =>
                    exception.Message.Contains("Version mismatch error", StringComparison.OrdinalIgnoreCase)),
            DelayGenerator = args => new ValueTask<TimeSpan?>(
                GetDelay(null, args.Outcome.Exception, args.AttemptNumber))
        };

        return new ResiliencePipelineBuilder()
            .AddRetry(options)
            .Build();
    }

    internal static bool ShouldRetryRestResponse(RestResponse response)
    {
        if (response.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.ServiceUnavailable)
        {
            return true;
        }

        if (response.StatusCode != HttpStatusCode.TooManyRequests)
        {
            return false;
        }

        return !TryGetServerDelay(response, out var delay) || IsAcceptableServerDelay(delay);
    }

    internal static bool TryGetServerDelay(RestResponse response, out TimeSpan delay)
    {
        if (TryGetHeaderValue(response, "X-Contentful-RateLimit-Reset", out var resetValue) &&
            TryParseDelay(resetValue, out delay))
        {
            return true;
        }

        if (TryGetHeaderValue(response, "Retry-After", out var retryAfterValue))
        {
            if (TryParseDelay(retryAfterValue, out delay))
            {
                return true;
            }

            if (DateTimeOffset.TryParse(retryAfterValue, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var retryAt))
            {
                delay = retryAt - DateTimeOffset.UtcNow;
                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.Zero;
                }

                return true;
            }
        }

        delay = default;
        return false;
    }

    private static TimeSpan GetDelay(RestResponse? response, Exception? exception, int attemptNumber)
    {
        TimeSpan delay;

        if (response is not null && TryGetServerDelay(response, out var responseDelay))
        {
            delay = responseDelay;
        }
        else if (exception is ContentfulRateLimitException rateLimitException)
        {
            delay = TimeSpan.FromSeconds(Math.Max(0, rateLimitException.SecondsUntilNextRequest));
        }
        else
        {
            var exponentialSeconds = Math.Min(
                Math.Pow(2, attemptNumber),
                MaximumFallbackDelay.TotalSeconds);
            delay = TimeSpan.FromSeconds(exponentialSeconds);
        }

        return delay + TimeSpan.FromMilliseconds(Random.Shared.NextDouble() * MaximumJitter.TotalMilliseconds);
    }

    private static bool ShouldRetryHttpRequestException(HttpRequestException exception) =>
        exception.StatusCode is HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.ServiceUnavailable;

    private static bool IsAcceptableServerDelay(TimeSpan delay) => delay <= MaximumServerDelay;

    private static bool TryGetHeaderValue(RestResponse response, string name, out string value)
    {
        value = response.Headers?
            .FirstOrDefault(header => header.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
            ?.Value?.ToString() ?? string.Empty;

        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryParseDelay(string value, out TimeSpan delay)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) &&
            seconds >= 0)
        {
            delay = TimeSpan.FromSeconds(seconds);
            return true;
        }

        delay = default;
        return false;
    }
}
