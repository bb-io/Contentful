using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryCreatedHandler : BaseWebhookHandler
    {
        public EntryCreatedHandler([WebhookParameter] string spaceId) : base("Entry", "create", spaceId)
        {
        }
    }
}
