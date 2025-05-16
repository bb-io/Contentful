using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Contentful.Webhooks.Handlers.AssetHandlers;

public class AssetUnpublishedHandler(InvocationContext invocationContext, [WebhookParameter(true)] WebhookInput input) 
    : BaseWebhookHandler(invocationContext, "Asset", "unpublish", input)
{ }