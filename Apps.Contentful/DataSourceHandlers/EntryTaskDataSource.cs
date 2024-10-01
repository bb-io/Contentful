using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Dtos;
using Apps.Contentful.Models.Entities;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Contentful.DataSourceHandlers;

public class EntryTaskDataSource(InvocationContext invocationContext, [ActionParameter] EntryTaskIdentifier identifier) : ContentfulInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (identifier.EntryId == null!)
        {
            throw new Exception("You should provide an Entry ID first.");
        }
        
        var client = new ContentfulRestClient(Creds, identifier.Environment);
        var request = new ContentfulRestRequest($"/entries/{identifier.EntryId}/tasks", Method.Get, Creds);
        var entryTasks = await client.Paginate<TaskDto>(request);

        var taskDtos = entryTasks as TaskDto[] ?? entryTasks.ToArray();
        
        return taskDtos
            .Select(x => new EntryTaskEntity(x))
            .Where(x => context.SearchString is null ||
                        x.Body.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Id, x => x.Body);
    }
}