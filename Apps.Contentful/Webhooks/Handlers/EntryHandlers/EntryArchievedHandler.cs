using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntryArchivedHandler : BaseWebhookHandler
{
    public EntryArchivedHandler([WebhookParameter] WebhookInput input) : base("Entry", "archive", input)
    {
    }
}