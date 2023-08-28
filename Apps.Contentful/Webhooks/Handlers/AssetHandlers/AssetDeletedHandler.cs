namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class AssetDeletedHandler : BaseWebhookHandler
    {
        public AssetDeletedHandler() : base("Asset", "delete")
        {
        }
    }
}
