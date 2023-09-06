using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers
{
    public class AssetCreatedHandler : BaseWebhookHandler
    {
        public AssetCreatedHandler([WebhookParameter] SpaceIdentifier space) : base("Asset", "create", space)
        {
        }
    }
}
