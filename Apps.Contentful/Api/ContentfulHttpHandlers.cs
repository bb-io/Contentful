namespace Apps.Contentful.Api;

public static class ContentfulHttpHandlers
{
    public static HttpMessageHandler Wrap(HttpMessageHandler transportHandler) =>
        new ContentfulErrorHandler(new ContentfulRetryHandler(transportHandler));
}
