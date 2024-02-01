using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers;

public class AssetUnpublishedHandler : BaseWebhookHandler
{
    public AssetUnpublishedHandler([WebhookParameter(true)] WebhookInput input) : base("Asset", "unpublish", input)
    {
    }
}