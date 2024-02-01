using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntrySavedHandler : BaseWebhookHandler
{
    public EntrySavedHandler([WebhookParameter(true)] WebhookInput input) : base("Entry", "save", input)
    {
    }
}