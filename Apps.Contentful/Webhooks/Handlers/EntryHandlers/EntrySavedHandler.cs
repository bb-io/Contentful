using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntrySavedHandler : BaseWebhookHandler
    {
        public EntrySavedHandler([WebhookParameter] SpaceIdentifier space) : base("Entry", "save", space)
        {
        }
    }
}
