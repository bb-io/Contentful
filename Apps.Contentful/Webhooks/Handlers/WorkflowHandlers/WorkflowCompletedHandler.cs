using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.WorkflowHandlers;

public class WorkflowCompletedHandler(InvocationContext invocationContext, [WebhookParameter(true)] WebhookInput input)
    : BaseWebhookHandler(invocationContext, "Workflow", "complete", input);