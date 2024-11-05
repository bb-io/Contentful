using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Dtos;
using Apps.Contentful.Models.Identifiers;
using Apps.Contentful.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Contentful.DataSourceHandlers;

public class WorkflowStepDataHandler(
    InvocationContext invocationContext,
    [ActionParameter] EnvironmentIdentifier identifier,
    [ActionParameter] WorkflowStepFilterRequest workflowStepFilterRequest)
    : ContentfulInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(workflowStepFilterRequest.WorkflowDefinitionId))
        {
            throw new InvalidOperationException("You should provide a workflow definition ID first");
        }
        
        var client = new ContentfulRestClient(Creds, identifier.Environment);
        var request = new ContentfulRestRequest($"/workflow_definitions/{workflowStepFilterRequest.WorkflowDefinitionId}", Method.Get, Creds);
        var workflowDefinition = await client.ExecuteWithErrorHandling<WorkflowDefinitionDto>(request);

        return workflowDefinition.Steps
            .Where(x => context.SearchString is null ||
                        x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.StepId, x => x.Name);
    }
}