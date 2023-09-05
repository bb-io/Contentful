using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryUnpublishedHandler : BaseWebhookHandler
    {
        public EntryUnpublishedHandler([WebhookParameter] string spaceId) : base("Entry", "unpublish", spaceId)
        {
        }
    }
}
