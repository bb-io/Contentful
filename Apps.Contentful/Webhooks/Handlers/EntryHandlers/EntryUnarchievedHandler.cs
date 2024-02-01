using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntryUnarchivedHandler : BaseWebhookHandler
{
    public EntryUnarchivedHandler([WebhookParameter(true)] WebhookInput input) : base("Entry", "unarchive", input)
    {
    }
}