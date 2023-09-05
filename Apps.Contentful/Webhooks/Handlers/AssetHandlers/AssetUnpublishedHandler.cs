using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class AssetUnpublishedHandler : BaseWebhookHandler
    {
        public AssetUnpublishedHandler([WebhookParameter] string spaceId) : base("Asset", "unpublish", spaceId)
        {
        }
    }
}
