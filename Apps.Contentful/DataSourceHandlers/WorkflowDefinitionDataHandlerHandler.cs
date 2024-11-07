using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Dtos;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Contentful.DataSourceHandlers;

public class WorkflowDefinitionDataHandler(InvocationContext invocationContext, [ActionParameter] EnvironmentIdentifier identifier) : ContentfulInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulRestClient(Creds, identifier.Environment);
        var request = new ContentfulRestRequest("/workflow_definitions", Method.Get, Creds);
        var items = await client.Paginate<WorkflowDefinitionDto>(request);
        
        var workflowDefinitionDtos = items as WorkflowDefinitionDto[] ?? items.ToArray();
        
        return workflowDefinitionDtos
            .Where(x => context.SearchString is null ||
                        x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Sys.Id, x => x.Name);
    }
}