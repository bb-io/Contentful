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
    [Action("Search workflows", Description = "Search for active workflows based on the provided inputs")]
    public async Task<WorkflowsResponse> SearchWorkflowsAsync([ActionParameter] SearchWorkflowsRequest searchRequest)
    {
        var client = new ContentfulRestClient(Creds, searchRequest.Environment);
        var workflowsRequest = new ContentfulRestRequest("/workflows", Method.Get, Creds);
        var workflows = await client.Paginate<WorkflowDto>(workflowsRequest);

        var workflowResponses = workflows.Select(workflow => new WorkflowResponse
        {
            StepId = workflow.StepId,
            WorkflowDefinitionId = workflow.Sys.WorkflowDefinition.Sys.Id,
            Version = workflow.Sys.Version,
            EntityId = workflow.Sys.Entity.Sys.Id,
            WorkflowId = workflow.Sys.Id
        });

        if (!string.IsNullOrEmpty(searchRequest.WorkflowDefinitionId))
        {
            workflowResponses = workflowResponses
                .Where(workflow => workflow.WorkflowDefinitionId == searchRequest.WorkflowDefinitionId);
        }

        var workflowDefinitionIds = workflowResponses
            .Select(w => w.WorkflowDefinitionId)
            .Distinct()
            .ToList();

        var definitionTasks = workflowDefinitionIds.Select(id =>
        {
            var defRequest = new ContentfulRestRequest($"/workflow_definitions/{id}", Method.Get, Creds);
            return client.ExecuteWithErrorHandling<WorkflowDefinitionDto>(defRequest);
        });

        var workflowDefinitions = await Task.WhenAll(definitionTasks);

        var workflowDefinitionDict = workflowDefinitions
            .Where(def => def != null)
            .ToDictionary(def => def.Sys.Id, def => def);

        var workflowStepResponses = new List<WorkflowDefinitionResponse>();
        foreach (var workflowResponse in workflowResponses)
        {
            if (!workflowDefinitionDict.TryGetValue(workflowResponse.WorkflowDefinitionId, out var workflowDefinition))
            {
                continue;
            }

            var currentStep = workflowDefinition.Steps.FirstOrDefault(x => x.StepId == workflowResponse.StepId)
                              ?? new WorkflowStep();
            var currentStepIndex = workflowDefinition.Steps.IndexOf(currentStep);
            var nextStep = (currentStepIndex + 1) < workflowDefinition.Steps.Count
                ? workflowDefinition.Steps[currentStepIndex + 1]
                : null;
            var previousStep = (currentStepIndex - 1) >= 0
                ? workflowDefinition.Steps[currentStepIndex - 1]
                : null;

            workflowStepResponses.Add(new WorkflowDefinitionResponse
            {
                WorkflowId = workflowResponse.WorkflowId,
                WorkflowDefinitionId = workflowResponse.WorkflowDefinitionId,
                Name = workflowDefinition.Name,
                Description = workflowDefinition.Description,
                EntryId = workflowResponse.EntityId,
                CurrentStep = currentStep,
                NextStep = nextStep,
                PreviousStep = previousStep
            });
        }

        if (searchRequest.CurrentStepNames != null)
        {
            workflowStepResponses = workflowStepResponses.Where(x =>
                x.CurrentStep != null! &&
                searchRequest.CurrentStepNames.Contains(x.CurrentStep.Name))
                .ToList();
        }

        return new WorkflowsResponse(workflowStepResponses);
    }

    [Action("Get workflow", Description = "Returns details of a specific workflow based on the workflow ID")]
    public async Task<WorkflowResponse> GetWorkflowAsync([ActionParameter] WorkflowIdentifier workflowRequest)
    {
        var client = new ContentfulRestClient(Creds, workflowRequest.Environment);
        var request = new ContentfulRestRequest($"/workflows/{workflowRequest.WorkflowId}", Method.Get, Creds);
        var workflow = await client.ExecuteWithErrorHandling<WorkflowDto>(request);

        return new WorkflowResponse
        {
            StepId = workflow.StepId,
            WorkflowDefinitionId = workflow.Sys.WorkflowDefinition.Sys.Id,
            Version = workflow.Sys.Version,
            EntityId = workflow.Sys.Entity.Sys.Id,
            WorkflowId = workflow.Sys.Id
        };
    }

    [Action("Get workflow definition",
        Description = "Returns details of a specific workflow definition based on the workflow definition ID")]
    public async Task<FullWorkflowDefinitionResponse> GetWorkflowDefinitionAsync(
        [ActionParameter] WorkflowDefinitionIdentifier workflowDefinitionRequest)
    {
        var client = new ContentfulRestClient(Creds, workflowDefinitionRequest.Environment);
        var request =
            new ContentfulRestRequest($"/workflow_definitions/{workflowDefinitionRequest.WorkflowDefinitionId}",
                Method.Get, Creds);
        var workflowDefinition = await client.ExecuteWithErrorHandling<WorkflowDefinitionDto>(request);

        return new FullWorkflowDefinitionResponse
        {
            WorkflowDefinitionId = workflowDefinition.Sys.Id,
            Name = workflowDefinition.Name,
            Description = workflowDefinition.Description,
            Steps = workflowDefinition.Steps
        };
    }

    [Action("Update workflow step", Description = "Move a workflow to a specific step")]
    public async Task<WorkflowResponse> UpdateWorkflowStepAsync(
        [ActionParameter] UpdateWorkflowStepRequest updateRequest)
    {
        var client = new ContentfulRestClient(Creds, updateRequest.Environment);

        var workflowResponse = await GetWorkflowAsync(updateRequest);
        if(workflowResponse.StepId == updateRequest.StepId)
        {
            return workflowResponse;
        }

        var request = new ContentfulRestRequest($"/workflows/{updateRequest.WorkflowId}", Method.Put, Creds)
            .WithJsonBody(new { stepId = updateRequest.StepId })
            .AddHeader("X-Contentful-Version", workflowResponse.Version.ToString());

        var workflow = await client.ExecuteWithErrorHandling<WorkflowDto>(request);

        return new WorkflowResponse
        {
            StepId = workflow.StepId,
            WorkflowDefinitionId = workflow.Sys.WorkflowDefinition.Sys.Id,
            Version = workflow.Sys.Version,
            EntityId = workflow.Sys.Entity.Sys.Id,
            WorkflowId = workflow.Sys.Id
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