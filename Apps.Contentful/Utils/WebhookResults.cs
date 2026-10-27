using System.Net;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Utils;

public static class WebhookResults
{
    public static WebhookResponse<T> Preflight<T>() where T : class
    {
        return new()
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = null,
            ReceivedWebhookRequestType = WebhookRequestType.Preflight,
        };
    }
}