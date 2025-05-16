using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryTaskHandlers;

public class EntryTaskCreatedHandler(InvocationContext invocationContext, [WebhookParameter(true)] WebhookInput input)
    : BaseWebhookHandler(invocationContext, "Task", "create", input);