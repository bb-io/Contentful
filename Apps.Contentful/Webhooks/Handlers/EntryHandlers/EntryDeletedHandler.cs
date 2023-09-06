namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers
{
    public class EntryDeletedHandler : BaseWebhookHandler
    {
        public EntryDeletedHandler() : base("Entry", "delete")
        {
        }
    }
}
