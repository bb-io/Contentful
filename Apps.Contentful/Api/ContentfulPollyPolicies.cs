using System.Globalization;
using System.Net;
using Polly;
using Polly.Retry;

namespace Apps.Contentful.Api;

public static class ContentfulPollyPolicies
{
    internal const int DefaultRetryCount = 6;
    internal static readonly TimeSpan MaximumServerDelay = TimeSpan.FromSeconds(60);

    private static readonly TimeSpan MaximumBackoffDelay = TimeSpan.FromSeconds(30);
    private const double MaximumJitterRatio = 0.5;

    public static ResiliencePipeline<HttpResponseMessage> CreateHttpPipeline()
    {
        var options = new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = DefaultRetryCount,
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>().HandleResult(ShouldRetry),
            DelayGenerator = args => new ValueTask<TimeSpan?>(GetDelay(
                args.Outcome.Result is { } response && TryGetServerDelay(response, out var serverDelay)
                    ? serverDelay
                    : null,
                args.AttemptNumber))
        };

        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(options)
            .Build();
    }

    internal static bool TryGetServerDelay(HttpResponseMessage response, out TimeSpan delay)
    {
        if (GetHeaderValue(response, "X-Contentful-RateLimit-Reset") is { } resetValue &&
            TryParseDelay(resetValue, out delay))
        {
            return true;
        }

        if (GetHeaderValue(response, "Retry-After") is { } retryAfterValue)
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

    private static bool ShouldRetry(HttpResponseMessage response)
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

    private static TimeSpan GetDelay(TimeSpan? serverDelay, int attemptNumber)
    {
        var backoffDelay = TimeSpan.FromSeconds(Math.Min(
            Math.Pow(2, attemptNumber),
            MaximumBackoffDelay.TotalSeconds));
        var delay = serverDelay > backoffDelay ? serverDelay.Value : backoffDelay;

        return delay * (1 + Random.Shared.NextDouble() * MaximumJitterRatio);
    }

    private static bool IsAcceptableServerDelay(TimeSpan delay) => delay <= MaximumServerDelay;

    private static string? GetHeaderValue(HttpResponseMessage response, string name)
    {
        var value = response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

        return string.IsNullOrWhiteSpace(value) ? null : value;
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
