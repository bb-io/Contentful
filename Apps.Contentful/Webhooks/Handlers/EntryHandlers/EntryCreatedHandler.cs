using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryCreatedHandler : BaseWebhookHandler
    {
        public EntryCreatedHandler([WebhookParameter] SpaceIdentifier space) : base("Entry", "create", space)
        {
        }
    }
}
