using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntryPublishedHandler : BaseWebhookHandler
{
    public EntryPublishedHandler([WebhookParameter] WebhookInput input) : base("Entry", "publish", input)
    {
    }
}