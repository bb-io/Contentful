using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntrySavedHandler : BaseWebhookHandler
{
    public EntrySavedHandler([WebhookParameter] WebhookInput input) : base("Entry", "save", input)
    {
    }
}