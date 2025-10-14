using Apps.Contentful.Api;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class ReferenceFieldDataHandler(InvocationContext invocationContext, [ActionParameter] EntryIdentifier entryIdentifier) 
    : BaseInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(entryIdentifier.EntryId))
        {
            throw new Exception("Please provide Entry ID first");
        }
        
        var client = new ContentfulClient(invocationContext.AuthenticationCredentialsProviders, entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId, cancellationToken: cancellationToken));
        
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.ExecuteWithErrorHandling(async () => 
            await client.GetContentType(contentTypeId, cancellationToken: cancellationToken));
        
        var referenceFields = contentType.Fields
            .Where(f => f.LinkType == "Entry" || (f.Type == "Array" && f.Items?.LinkType == "Entry"))
            .Where(f => context.SearchString == null ||
                       f.Id.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase) ||
                       f.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(f => new DataSourceItem(f.Id, f.Name));
        
        return referenceFields;
    }
}
