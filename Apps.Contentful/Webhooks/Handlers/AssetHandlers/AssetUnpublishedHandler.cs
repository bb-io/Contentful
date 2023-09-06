using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers
{
    public class AssetUnpublishedHandler : BaseWebhookHandler
    {
        public AssetUnpublishedHandler([WebhookParameter] SpaceIdentifier space) : base("Asset", "unpublish", space)
        {
        }
    }
}
