using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers;

public class AssetArchivedHandler : BaseWebhookHandler
{
    public AssetArchivedHandler([WebhookParameter] WebhookInput input) : base("Asset", "archive", input)
    {
    }
}