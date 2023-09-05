using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class AssetPublishedHandler : BaseWebhookHandler
    {
        public AssetPublishedHandler([WebhookParameter] string spaceId) : base("Asset", "publish", spaceId)
        {
        }
    }
}
