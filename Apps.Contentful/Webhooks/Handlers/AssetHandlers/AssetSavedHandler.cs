using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers
{
    public class AssetSavedHandler : BaseWebhookHandler
    {
        public AssetSavedHandler([WebhookParameter] SpaceIdentifier space) : base("Asset", "save", space)
        {
        }
    }
}
