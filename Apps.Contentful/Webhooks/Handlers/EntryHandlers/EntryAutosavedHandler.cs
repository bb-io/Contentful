using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntryAutoSavedHandler : BaseWebhookHandler
{
    public EntryAutoSavedHandler([WebhookParameter(true)] WebhookInput input) : base("Entry", "auto_save", input)
    {
    }
}