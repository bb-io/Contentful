using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class LocaleDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public LocaleDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders);
        var locales = (await client.GetLocalesCollection(cancellationToken: cancellationToken))
            .Where(l => context.SearchString == null ||
                        l.Code.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));
        return locales.ToDictionary(l => l.Code, l => l.Code);
    }
}