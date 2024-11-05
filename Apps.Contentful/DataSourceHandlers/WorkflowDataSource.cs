using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Dtos;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Contentful.DataSourceHandlers;

public class WorkflowDataSource(InvocationContext invocationContext, [ActionParameter] EnvironmentIdentifier identifier)
    : ContentfulInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulRestClient(Creds, identifier.Environment);
        var request = new ContentfulRestRequest($"/workflows", Method.Get, Creds);
        var items = await client.Paginate<WorkflowDto>(request);

        var workflowDtos = items as WorkflowDto[] ?? items.ToArray();

        return workflowDtos
            .Where(x => context.SearchString is null ||
                        x.Sys.Id.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Sys.Id, x => x.Sys.Id);
    }
}