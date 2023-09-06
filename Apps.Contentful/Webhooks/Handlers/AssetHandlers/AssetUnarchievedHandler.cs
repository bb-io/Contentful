using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers
{
    public class AssetUnarchivedHandler : BaseWebhookHandler
    {
        public AssetUnarchivedHandler([WebhookParameter] SpaceIdentifier space) : base("Asset", "unarchive", space)
        {
        }
    }
}
