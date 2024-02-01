using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers;

public class AssetSavedHandler : BaseWebhookHandler
{
    public AssetSavedHandler([WebhookParameter(true)] WebhookInput input) : base("Asset", "save", input)
    {
    }
}