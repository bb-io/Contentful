namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers;

public class AssetCreatedHandler : BaseWebhookHandler
{
    public AssetCreatedHandler() : base("Asset", "create")
    {
    }
}