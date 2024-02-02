using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntryDeletedHandler : BaseWebhookHandler
{
    public EntryDeletedHandler([WebhookParameter(true)] WebhookInput input) : base("Entry", "delete", input)
    {
    }
}