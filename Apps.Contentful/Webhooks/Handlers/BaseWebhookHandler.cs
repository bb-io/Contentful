using Apps.Contentful.Api;
using Apps.Contentful.Utils;
using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Contentful.Core.Models.Management;

namespace Apps.Contentful.Webhooks.Handlers;

public class BaseWebhookHandler : BaseInvocable, IWebhookEventHandler
{
    private readonly string? _entityName;
    private readonly string? _actionName;
    private readonly WebhookInput _webhookInput;
    private readonly List<string>? _topics;

    protected BaseWebhookHandler(InvocationContext invocationContext, string entityName, string actionName, [WebhookParameter(true)] WebhookInput input)
        : base(invocationContext)
    {
        _entityName = entityName;
        _actionName = actionName;
        _webhookInput = input;
    }
    
    protected BaseWebhookHandler(InvocationContext invocationContext, List<string> topics, [WebhookParameter(true)] WebhookInput input)
        : base(invocationContext)
    {
        _webhookInput = input;
        _topics = topics;
    }

    public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider,
        Dictionary<string, string> values)
    {
        try
        {
            await WebhookLogger.LogAsync(new
            {
                message = "Subscribing to webhook",
                values,
                authenticationCredentialsProvider
            });
        
            var filters = _webhookInput.Environment is null
                ? null
                : new List<IConstraint>
                {
                    new EqualsConstraint
                    {
                        Property = "sys.environment.sys.id",
                        ValueToEqual = _webhookInput.Environment
                    }
                };

            var client = new ContentfulClient(authenticationCredentialsProvider, _webhookInput.Environment);
            var name = InvocationContext.Tenant?.Name ??
                       $"{_entityName?.ToUpper()}_{_actionName?.ToUpper()}_{Guid.NewGuid()}";
        
            await client.CreateWebhook(new Webhook
            {
                Name = name,
                Url = values["payloadUrl"],
                Topics = _topics ?? [$"{_entityName}.{_actionName}"],
                Filters = filters,
                HttpBasicUsername = null,
                HttpBasicPassword = null
            });
        }
        catch (Exception e)
        {
            await WebhookLogger.LogAsync(new
            {
                message = "Error subscribing to webhook",
                error = e.Message,
                stack_trace = e.StackTrace,
                values
            });
            throw;
        }
    }

    public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider,
        Dictionary<string, string> values)
    {
        try
        {
            await WebhookLogger.LogAsync(new
            {
                message = "Unsubscribing from webhook",
                values,
                authenticationCredentialsProvider
            });

            var client = new ContentfulClient(authenticationCredentialsProvider, _webhookInput.Environment);
            var webhooks = await client.GetWebhooksCollection();

            var webhook = webhooks.FirstOrDefault(w => w.Url == values["payloadUrl"]);
            if (webhook != null)
            {
                await client.DeleteWebhook(webhook.SystemProperties.Id);
            }
        }
        catch (Exception e)
        {
            await WebhookLogger.LogAsync(new
            {
                message = "Error unsubscribing from webhook",
                error = e.Message,
                stack_trace = e.StackTrace,
                values
            });
            throw;
        }
    }
}