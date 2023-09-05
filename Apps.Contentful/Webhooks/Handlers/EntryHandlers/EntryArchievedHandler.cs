using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryArchievedHandler : BaseWebhookHandler
    {
        public EntryArchievedHandler([WebhookParameter] string spaceId) : base("Entry", "archive", spaceId)
        {
        }
    }
}
