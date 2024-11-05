using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.WorkflowHandlers;

public class WorkflowCompletedHandler([WebhookParameter(true)] WebhookInput input)
    : BaseWebhookHandler("Workflow", "completed", input);