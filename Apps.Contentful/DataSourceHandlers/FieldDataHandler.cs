using Apps.Contentful.Api;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.DataSourceHandlers;

public class FieldDataHandler(InvocationContext invocationContext, [ActionParameter] EntryLocaleIdentifier entryIdentifier) 
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
        var fields = (JObject)entry.Fields;
        var availableFields = fields.Properties()
            .Where(x => context.SearchString == null ||
                        x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Where(x => entryIdentifier.Locale == null! || x.Value[entryIdentifier.Locale] != null)
            .Select(f => f.Name);
        
        return availableFields.Select(f => new DataSourceItem(f, f));
    }
}