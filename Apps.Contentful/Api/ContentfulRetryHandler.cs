using Polly;

namespace Apps.Contentful.Api;

public class ContentfulRetryHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    private static readonly ResiliencePipeline<HttpResponseMessage> RetryPipeline =
        ContentfulPollyPolicies.CreateHttpPipeline();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!CanBeResent(request))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        return await RetryPipeline.ExecuteAsync(
            async token => await base.SendAsync(request, token),
            cancellationToken);
    }

    private static bool CanBeResent(HttpRequestMessage request) =>
        request.Content is null or ByteArrayContent;
}
