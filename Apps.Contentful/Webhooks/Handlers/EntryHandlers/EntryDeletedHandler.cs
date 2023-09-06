using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryDeletedHandler : BaseWebhookHandler
    {
        public EntryDeletedHandler([WebhookParameter] SpaceIdentifier space) : base("Entry", "delete", space)
        {
        }
    }
}
