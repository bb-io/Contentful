using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Dtos;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Apps.Contentful.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using RestSharp;

namespace Apps.Contentful.Actions;

[ActionList]
public class WorkflowActions(InvocationContext invocationContext) : ContentfulInvocable(invocationContext)
{
    [Action("Get workflow", Description = "Returns details of a specific workflow based on the workflow ID")]
    public async Task<WorkflowResponse> GetWorkflowAsync([ActionParameter] WorkflowIdentifier workflowRequest)
    {
        var client = new ContentfulRestClient(Creds, workflowRequest.Environment);
        var request = new ContentfulRestRequest($"/workflows/{workflowRequest.WorkflowId}", Method.Get, Creds);
        var workflow = await client.ExecuteWithErrorHandling<WorkflowDto>(request);

        return new WorkflowResponse()
        {
            StepId = workflow.StepId,
            WorkflowDefinitionId = workflow.Sys.WorkflowDefinition.Sys.Id,
            Version = workflow.Sys.Version,
            EntityId = workflow.Sys.Entity.Sys.Id
        };
    }
    
    [Action("Update workflow step", Description = "Move a workflow to a specific step")]
    public async Task<WorkflowResponse> UpdateWorkflowStepAsync([ActionParameter] UpdateWorkflowStepRequest updateRequest)
    {
        var client = new ContentfulRestClient(Creds, updateRequest.Environment);
        
        var workflowResponse = await GetWorkflowAsync(updateRequest);
        
        var request = new ContentfulRestRequest($"/workflows/{updateRequest.WorkflowId}", Method.Put, Creds)
            .WithJsonBody(new { stepId = updateRequest.StepId })
            .AddHeader("X-Contentful-Version", workflowResponse.Version.ToString());
        
        var workflow = await client.ExecuteWithErrorHandling<WorkflowDto>(request);

        return new WorkflowResponse
        {
            StepId = workflow.StepId,
            WorkflowDefinitionId = workflow.Sys.WorkflowDefinition.Sys.Id,
            Version = workflow.Sys.Version,
            EntityId = workflow.Sys.Entity.Sys.Id
        };
    }
    
    [Action("Complete workflow", Description = "Complete a workflow")]
    public async Task CompleteWorkflowAsync([ActionParameter] WorkflowIdentifier workflowRequest)
    {
        var client = new ContentfulRestClient(Creds, workflowRequest.Environment);
        
        var workflowResponse = await GetWorkflowAsync(workflowRequest);
        var request = new ContentfulRestRequest($"/workflows/{workflowRequest.WorkflowId}/complete", Method.Put, Creds)
            .AddHeader("X-Contentful-Version", workflowResponse.Version.ToString());
        await client.ExecuteWithErrorHandling(request);
    }
    
    [Action("Cancel workflow", Description = "Cancel a workflow")]
    public async Task CancelWorkflowAsync([ActionParameter] WorkflowIdentifier workflowRequest)
    {
        var client = new ContentfulRestClient(Creds, workflowRequest.Environment);
        
        var workflowResponse = await GetWorkflowAsync(workflowRequest);
        
        var request = new ContentfulRestRequest($"/workflows/{workflowRequest.WorkflowId}", Method.Delete, Creds)
            .AddHeader("X-Contentful-Version", workflowResponse.Version.ToString());
        await client.ExecuteWithErrorHandling(request);
    }
}