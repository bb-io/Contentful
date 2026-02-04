using Apps.Contentful.Api;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class LocaleDataSourceHandler(
    InvocationContext invocationContext, 
    [ActionParameter] EnvironmentIdentifier identifier) 
    : BaseInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders, identifier.Environment);
        var locales = await client.GetLocalesCollection(cancellationToken: cancellationToken);

        var filtered = locales.Where(l => 
            context.SearchString == null || 
            l.Code.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)
        );
        return locales.Select(x => new DataSourceItem(x.Code, x.Name)).ToList();
    }
}