using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Webhooks.Models.Payload;

public class EntityWebhookResponse
{
    [Display("ID")]
    public string Id { get; set; }
}