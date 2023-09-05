using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryPublishedHandler : BaseWebhookHandler
    {
        public EntryPublishedHandler([WebhookParameter] string spaceId) : base("Entry", "publish", spaceId)
        {
        }
    }
}
