using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers
{
    public class AssetDeletedHandler : BaseWebhookHandler
    {
        public AssetDeletedHandler([WebhookParameter] SpaceIdentifier space) : base("Asset", "delete", space)
        {
        }
    }
}
