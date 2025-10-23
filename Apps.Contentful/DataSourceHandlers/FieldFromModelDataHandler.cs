using Apps.Contentful.Api;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class FieldFromModelDataHandler(InvocationContext invocationContext, [ActionParameter] ContentModelIdentifier modelIdentifier) 
    : BaseInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(modelIdentifier.ContentModelId))
        {
            throw new Exception("Please provide Model ID first");
        }
        
        var client = new ContentfulClient(invocationContext.AuthenticationCredentialsProviders, modelIdentifier.Environment);

        var model = await client.ExecuteWithErrorHandling(async () =>
            await client.GetContentType(modelIdentifier.ContentModelId, cancellationToken: cancellationToken));

        return model.Fields
            .Where(x => context.SearchString == null ||
                        x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(f => new DataSourceItem(f.Id, f.Name));
    }
}