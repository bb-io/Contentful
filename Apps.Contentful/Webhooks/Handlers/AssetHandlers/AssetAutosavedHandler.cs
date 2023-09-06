using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers
{
    public class AssetAutoSavedHandler : BaseWebhookHandler
    {
        public AssetAutoSavedHandler([WebhookParameter] SpaceIdentifier space) : base("Asset", "auto_save", space)
        {
        }
    }
}
