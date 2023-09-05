using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class AssetUnarchievedHandler : BaseWebhookHandler
    {
        public AssetUnarchievedHandler([WebhookParameter] string spaceId) : base("Asset", "unarchive", spaceId)
        {
        }
    }
}
