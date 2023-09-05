using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class AssetDeletedHandler : BaseWebhookHandler
    {
        public AssetDeletedHandler([WebhookParameter] string spaceId) : base("Asset", "delete", spaceId)
        {
        }
    }
}
