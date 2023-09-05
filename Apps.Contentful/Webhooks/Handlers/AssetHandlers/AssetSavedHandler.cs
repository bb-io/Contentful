using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class AssetSavedHandler : BaseWebhookHandler
    {
        public AssetSavedHandler([WebhookParameter] string spaceId) : base("Asset", "save", spaceId)
        {
        }
    }
}
