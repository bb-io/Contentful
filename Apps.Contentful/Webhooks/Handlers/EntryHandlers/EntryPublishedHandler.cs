using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntryPublishedHandler : BaseWebhookHandler
{
    public EntryPublishedHandler([WebhookParameter(true)] OptionalTagListIdentifier input) : base("Entry", "publish", new()
    {
        Environment = input.Environment
    })
    {
    }
}