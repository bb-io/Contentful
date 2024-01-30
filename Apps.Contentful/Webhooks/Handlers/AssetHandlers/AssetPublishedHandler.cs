using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers;

public class AssetPublishedHandler : BaseWebhookHandler
{
    public AssetPublishedHandler([WebhookParameter] WebhookInput input) : base("Asset", "publish", input)
    {
    }
}