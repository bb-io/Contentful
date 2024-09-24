using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.EntryTaskHandlers;

public class EntryTaskSavedHandler([WebhookParameter(true)] WebhookInput input)
    : BaseWebhookHandler("Task", "save", input);