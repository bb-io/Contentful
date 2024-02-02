using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers;

public class AssetCreatedHandler : BaseWebhookHandler
{
    public AssetCreatedHandler([WebhookParameter(true)] WebhookInput input) : base("Asset", "create", input)
    {
    }
}