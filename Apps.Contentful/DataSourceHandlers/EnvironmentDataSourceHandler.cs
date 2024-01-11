using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class EnvironmentDataSourceHandler : ContentfulInvocable, IAsyncDataSourceHandler
{
    public EnvironmentDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulClient(Creds, null);
        var environments = await client.GetEnvironments(cancellationToken: cancellationToken);

        return environments
            .Where(x => context.SearchString is null ||
                        x.SystemProperties.Id.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.SystemProperties.Id, x => x.SystemProperties.Id);
    }
}