using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class AssetAutosavedHandler : BaseWebhookHandler
    {
        public AssetAutosavedHandler([WebhookParameter] string spaceId) : base("Asset", "auto_save", spaceId)
        {
        }
    }
}
