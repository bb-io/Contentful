using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryUnarchivedHandler : BaseWebhookHandler
    {
        public EntryUnarchivedHandler([WebhookParameter] SpaceIdentifier space) : base("Entry", "unarchive", space)
        {
        }
    }
}
