using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers
{
    public class AssetArchivedHandler : BaseWebhookHandler
    {
        public AssetArchivedHandler([WebhookParameter] SpaceIdentifier space) : base("Asset", "archive", space)
        {
        }
    }
}
