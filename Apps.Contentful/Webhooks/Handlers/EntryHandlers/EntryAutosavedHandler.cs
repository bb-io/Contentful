using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryAutoSavedHandler : BaseWebhookHandler
    {
        public EntryAutoSavedHandler([WebhookParameter] SpaceIdentifier space) : base("Entry", "auto_save", space)
        {
        }
    }
}
