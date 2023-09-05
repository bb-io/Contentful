using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryUnarchievedHandler : BaseWebhookHandler
    {
        public EntryUnarchievedHandler([WebhookParameter] string spaceId) : base("Entry", "unarchive", spaceId)
        {
        }
    }
}
