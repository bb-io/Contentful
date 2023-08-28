namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class AssetCreatedHandler : BaseWebhookHandler
    {
        public AssetCreatedHandler() : base("Asset", "create")
        {
        }
    }
}
