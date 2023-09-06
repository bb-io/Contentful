using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class ContentModelDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public ContentModelDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders);
        var contentModels = (await client.GetContentTypes(cancellationToken: cancellationToken))
            .Where(m => context.SearchString == null ||
                        m.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));
        return contentModels.ToDictionary(m => m.SystemProperties.Id, m => m.Name);
    }
}