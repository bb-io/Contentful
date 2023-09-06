namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryCreatedHandler : BaseWebhookHandler
    {
        public EntryCreatedHandler() : base("Entry", "create")
        {
        }
    }
}
