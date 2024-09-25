using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryTaskHandlers;

public class EntryTaskCreatedHandler([WebhookParameter(true)] WebhookInput input)
    : BaseWebhookHandler("Task", "create", input);