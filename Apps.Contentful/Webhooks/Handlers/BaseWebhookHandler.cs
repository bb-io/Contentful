using Apps.Contentful.Api;
using Apps.Contentful.Webhooks.Models.Inputs;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;
using RestSharp;

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
        var filters = _webhookInput.Environment is null
            ? null
            : new List<object>
            {
                new
                {
                    @equals = new[]
                    {
                        new { doc = "sys.environment.sys.id" },
                        new { doc = _webhookInput.Environment }
                    }
                }
            };

        var name = InvocationContext.Tenant?.Name ??
                   $"{_entityName?.ToUpper()}_{_actionName?.ToUpper()}_{Guid.NewGuid()}";
            
        var webhookPayload = new
        {
            name = name,
            url = values["payloadUrl"],
            topics = _topics ?? new List<string> { $"{_entityName}.{_actionName}" },
            filters = filters
        };

        var client = new ContentfulRestClient(authenticationCredentialsProvider.ToArray(), _webhookInput.Environment);
        var request = new ContentfulRestRequest("/webhook_definitions", Method.Post, authenticationCredentialsProvider)
            .AddJsonBody(JsonConvert.SerializeObject(webhookPayload));
        await client.ExecuteWithErrorHandling(request);
    }

    public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider,
        Dictionary<string, string> values)
    {
        var client = new ContentfulRestClient(authenticationCredentialsProvider.ToArray(), _webhookInput.Environment);
        var getRequest = new ContentfulRestRequest("/webhook_definitions", Method.Get, authenticationCredentialsProvider);
        
        var response = await client.ExecuteWithErrorHandling(getRequest);
        if (string.IsNullOrEmpty(response.Content))
            return;
            
        var webhooksData = JsonConvert.DeserializeObject<dynamic>(response.Content);
        if (webhooksData?.items == null)
            return;
        
        string? webhookId = null;
        foreach (var webhook in webhooksData.items)
        {
            if (webhook?.url?.ToString() == values["payloadUrl"])
            {
                webhookId = webhook?.sys?.id?.ToString();
                break;
            }
        }

        if (!string.IsNullOrEmpty(webhookId))
        {
            var deleteRequest = new ContentfulRestRequest($"/webhook_definitions/{webhookId}", Method.Delete, authenticationCredentialsProvider);
            await client.ExecuteWithErrorHandling(deleteRequest);
        }
    }
}