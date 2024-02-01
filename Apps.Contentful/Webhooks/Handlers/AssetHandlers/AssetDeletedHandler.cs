using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers;

public class AssetDeletedHandler : BaseWebhookHandler
{
    public AssetDeletedHandler([WebhookParameter(true)] WebhookInput input) : base("Asset", "delete", input)
    {
    }
}