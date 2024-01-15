using Apps.Contentful.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers.Base;

public class BaseAssetDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private string? Environment { get; }

    public BaseAssetDataSourceHandler(InvocationContext invocationContext, string? environment) : base(
        invocationContext)
    {
        Environment = environment;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders, Environment);
        var assets =
            (await client.GetAssetsCollection($"?query={context.SearchString}", cancellationToken: cancellationToken))
            .Take(20);
        return assets.ToDictionary(a => a.SystemProperties.Id, a => a.Title.First().Value);
    }
}