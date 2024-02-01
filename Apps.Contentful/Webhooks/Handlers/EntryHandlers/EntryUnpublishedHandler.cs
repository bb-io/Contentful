using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntryUnpublishedHandler : BaseWebhookHandler
{
    public EntryUnpublishedHandler([WebhookParameter(true)] WebhookInput input) : base("Entry", "unpublish", input)
    {
    }
}