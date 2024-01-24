using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers;

public class AssetUnarchivedHandler : BaseWebhookHandler
{
    public AssetUnarchivedHandler([WebhookParameter] WebhookInput input) : base("Asset", "unarchive", input)
    {
    }
}