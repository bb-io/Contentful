using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntrySavedHandler : BaseWebhookHandler
    {
        public EntrySavedHandler([WebhookParameter] string spaceId) : base("Entry", "save", spaceId)
        {
        }
    }
}
