using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryArchivedHandler : BaseWebhookHandler
    {
        public EntryArchivedHandler([WebhookParameter] SpaceIdentifier space) : base("Entry", "archive", space)
        {
        }
    }
}
