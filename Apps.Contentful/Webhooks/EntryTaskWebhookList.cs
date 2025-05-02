using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Dtos;
using Apps.Contentful.Models.Entities;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Webhooks.Handlers.EntryTaskHandlers;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;

namespace Apps.Contentful.Webhooks;

[WebhookList]
public class EntryTaskWebhookList(InvocationContext invocationContext) : ContentfulInvocable(invocationContext)
{
    [Webhook("On entry task created", typeof(EntryTaskCreatedHandler), Description = "Triggers when a task is created for an entry")]
    public Task<WebhookResponse<EntryTaskEntity>> EntryTaskCreated(WebhookRequest webhookRequest,
        [WebhookParameter] EntryTaskFilterRequest request)
        => HandleWebhookResponse(webhookRequest, request);
    
    [Webhook("On entry task saved", typeof(EntryTaskSavedHandler), Description = "Triggers when a task is saved for an entry")]
    public Task<WebhookResponse<EntryTaskEntity>> EntryTaskSaved(WebhookRequest webhookRequest,
        [WebhookParameter] EntryTaskFilterRequest request)
        => HandleWebhookResponse(webhookRequest, request);
    
    private Task<WebhookResponse<EntryTaskEntity>> HandleWebhookResponse(WebhookRequest webhookRequest, EntryTaskFilterRequest request)
    {
        var content = webhookRequest.Body.ToString()!;
        var entryTask = JsonConvert.DeserializeObject<EntryTaskDto>(content)!;
        var entity = new EntryTaskEntity(entryTask.Sys.NewTask);

        if (request.UserId != null && entity.AssignedTo != request.UserId)
        {
            return Task.FromResult(new WebhookResponse<EntryTaskEntity>
            {
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            });
        }
        
        if (request.Body != null && entity.Body.Contains(request.Body, StringComparison.OrdinalIgnoreCase) == false)
        {
            return Task.FromResult(new WebhookResponse<EntryTaskEntity>
            {
                Result = null,
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            });
        }

        return Task.FromResult(new WebhookResponse<EntryTaskEntity>
        {
            Result = entity,
            ReceivedWebhookRequestType = WebhookRequestType.Default
        });
    }
}