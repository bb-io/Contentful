namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntryUnarchivedHandler : BaseWebhookHandler
{
    public EntryUnarchivedHandler() : base("Entry", "unarchive")
    {
    }
}