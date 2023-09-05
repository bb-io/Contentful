using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryDeletedHandler : BaseWebhookHandler
    {
        public EntryDeletedHandler([WebhookParameter] string spaceId) : base("Entry", "delete", spaceId)
        {
        }
    }
}
