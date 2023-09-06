using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers
{
    public class AssetPublishedHandler : BaseWebhookHandler
    {
        public AssetPublishedHandler([WebhookParameter] SpaceIdentifier space) : base("Asset", "publish", space)
        {
        }
    }
}
