using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Dtos;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Apps.Contentful.Webhooks.Handlers.WorkflowHandlers;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Contentful.Webhooks;

[WebhookList]
public class WorkflowWebhookList(InvocationContext invocationContext) : ContentfulInvocable(invocationContext)
{
    [Webhook("On workflow updated", typeof(WorkflowUpdatedHandler), Description = "Triggers when a workflow updated or created. Also triggers when a workflow changes its step.")]
    public Task<WebhookResponse<WorkflowDefinitionResponse>> WorkflowStepChanged(WebhookRequest webhookRequest,
        [WebhookParameter(true)] EnvironmentIdentifier input,
        [WebhookParameter] WorkflowStepFilterRequest request)
        => HandleWebhookResponse(webhookRequest, input, request);
    
    [Webhook("On workflow completed", typeof(WorkflowCompletedHandler), Description = "Triggers when a workflow is completed")]
    public Task<WebhookResponse<WorkflowDefinitionResponse>> WorkflowCompleted(WebhookRequest webhookRequest,
        [WebhookParameter(true)] EnvironmentIdentifier input,
        [WebhookParameter] WorkflowStepFilterRequest request)
        => HandleWebhookResponse(webhookRequest, input, request);

    private async Task<WebhookResponse<WorkflowDefinitionResponse>> HandleWebhookResponse(WebhookRequest webhookRequest,
        EnvironmentIdentifier environmentIdentifier,
        WorkflowStepFilterRequest request)
    {
        var content = webhookRequest.Body.ToString()!;
        var workflowDto = JsonConvert.DeserializeObject<WorkflowDto>(content)!;
        
        var workflowDefinitionRequest = new ContentfulRestRequest($"/workflow_definitions/{workflowDto.Sys.WorkflowDefinition.Sys.Id}", Method.Get, Creds);
        var client = new ContentfulRestClient(Creds, environmentIdentifier.Environment);
        var workflowDefinition = await client.ExecuteWithErrorHandling<WorkflowDefinitionDto>(workflowDefinitionRequest);

        var currentStep = workflowDefinition.Steps.FirstOrDefault(x => x.StepId == workflowDto.StepId)!;
        var nextStepIndex = workflowDefinition.Steps.IndexOf(currentStep) + 1;
        var nextStep = nextStepIndex < workflowDefinition.Steps.Count ? workflowDefinition.Steps[nextStepIndex] : null;
        
        var previousStep = workflowDefinition.Steps.FirstOrDefault(x => x.StepId == workflowDto.PreviousStepId);
        
        if (request.CurrentStepId != null && request.CurrentStepId != workflowDto.StepId)
        {
            return new WebhookResponse<WorkflowDefinitionResponse>
            {
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };
        }
        
        if (request.CurrentStepName != null && request.CurrentStepName != currentStep.Name)
        {
            return new WebhookResponse<WorkflowDefinitionResponse>
            {
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };
        }
        
        return new WebhookResponse<WorkflowDefinitionResponse>
        {
            Result = new()
            {
                WorkflowDefinitionId = workflowDefinition.Sys.Id,
                WorkflowId = workflowDto.Sys.Id,
                Name = workflowDefinition.Name,
                Description = workflowDefinition.Description,
                EntryId = workflowDto.Sys.Entity.Sys.Id,
                CurrentStep = currentStep ?? new(),
                PreviousStep = previousStep ?? null,
                NextStep = nextStep ?? null
            }
        };
    }
}