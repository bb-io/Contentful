using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryUnpublishedHandler : BaseWebhookHandler
    {
        public EntryUnpublishedHandler([WebhookParameter] SpaceIdentifier space) : base("Entry", "unpublish", space)
        {
        }
    }
}
