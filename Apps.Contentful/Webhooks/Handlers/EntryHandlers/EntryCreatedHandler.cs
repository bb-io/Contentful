using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntryCreatedHandler : BaseWebhookHandler
{
    public EntryCreatedHandler([WebhookParameter(true)] WebhookInput input) : base("Entry", "create", input)
    {
    }
}