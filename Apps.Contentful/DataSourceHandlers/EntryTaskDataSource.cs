using Apps.Contentful.Actions;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class EntryTaskDataSource(InvocationContext invocationContext, [ActionParameter] EntryIdentifier identifier) : ContentfulInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (identifier.EntryId == null!)
        {
            throw new Exception("You should provide an Entry ID first.");
        }
        
        var entryTaskActions = new EntryTaskActions(InvocationContext);
        var entryTasks = await entryTaskActions.GetEntryTasks(identifier);
        
        return entryTasks.EntryTasks
            .Where(x => context.SearchString is null ||
                        x.Body.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Id, x => x.Body);
    }
}