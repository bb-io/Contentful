using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers;

public class AssetAutoSavedHandler : BaseWebhookHandler
{
    public AssetAutoSavedHandler([WebhookParameter] WebhookInput input) : base("Asset", "auto_save", input)
    {
    }
}