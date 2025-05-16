using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryHandlers;

public class EntryAutoSavedHandler(InvocationContext invocationContext, [WebhookParameter(true)] WebhookInput input) 
    : BaseWebhookHandler(invocationContext, "Entry", "auto_save", input)
{ }