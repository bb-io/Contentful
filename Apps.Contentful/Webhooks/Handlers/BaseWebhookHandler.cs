using Apps.Contentful.Api;
using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Contentful.Core.Models.Management;

namespace Apps.Contentful.Webhooks.Handlers;

public class BaseWebhookHandler : IWebhookEventHandler
{
    private readonly string _entityName;
    private readonly string _actionName;
    private readonly WebhookInput _webhookInput;

    protected BaseWebhookHandler(string entityName, string actionName, [WebhookParameter(true)] WebhookInput input)
    {
        _entityName = entityName;
        _actionName = actionName;
        _webhookInput = input;
    }

    public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider,
        Dictionary<string, string> values)
    {
        var topic = $"{_entityName}.{_actionName}";
        var filters = _webhookInput.Environment is null
            ? null
            : new List<IConstraint>()
            {
                new EqualsConstraint()
                {
                    Property = "sys.environment.sys.id",
                    ValueToEqual = _webhookInput.Environment
                }
            };

        var client = new ContentfulClient(authenticationCredentialsProvider, null);
        await client.CreateWebhook(new Webhook
        {
            Name = topic,
            Url = values["payloadUrl"],
            Topics = new List<string> { topic },
            Filters = filters
        });
    }

    public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider,
        Dictionary<string, string> values)
    {
        var client = new ContentfulClient(authenticationCredentialsProvider, null);
        var topic = $"{_entityName}.{_actionName}";
        var webhooks = client.GetWebhooksCollection().Result;
        var webhook = webhooks.First(w => w.Name == topic);
        await client.DeleteWebhook(webhook.SystemProperties.Id);
    }
}