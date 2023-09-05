using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryAutosavedHandler : BaseWebhookHandler
    {
        public EntryAutosavedHandler([WebhookParameter] string spaceId) : base("Entry", "auto_save", spaceId)
        {
        }
    }
}
