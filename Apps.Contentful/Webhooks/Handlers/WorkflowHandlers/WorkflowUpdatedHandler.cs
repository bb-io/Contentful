using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.WorkflowHandlers;

public class WorkflowUpdatedHandler([WebhookParameter(true)] WebhookInput input)
    : BaseWebhookHandler(["Workflow.create", "Workflow.save"], input);