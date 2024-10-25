using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntryCreatedHandler : BaseWebhookHandler
{
    public EntryCreatedHandler([WebhookParameter(true)] OptionalTagListIdentifier input) : base("Entry", "create", new()
    {
        Environment = input.Environment
    })
    {
    }
}