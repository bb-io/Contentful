using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class AssetArchievedHandler : BaseWebhookHandler
    {
        public AssetArchievedHandler([WebhookParameter] string spaceId) : base("Asset", "archive", spaceId)
        {
        }
    }
}
