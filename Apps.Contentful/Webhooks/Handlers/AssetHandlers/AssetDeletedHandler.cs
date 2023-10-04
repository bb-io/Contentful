namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers;

public class AssetDeletedHandler : BaseWebhookHandler
{
    public AssetDeletedHandler() : base("Asset", "delete")
    {
    }
}