using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class AssetCreatedHandler : BaseWebhookHandler
    {
        public AssetCreatedHandler([WebhookParameter] string spaceId) : base("Asset", "create", spaceId)
        {
        }
    }
}
