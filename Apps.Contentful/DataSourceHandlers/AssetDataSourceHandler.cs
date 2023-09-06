using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class AssetDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public AssetDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders);
        var assets = (await client.GetAssetsCollection($"?query={context.SearchString}", cancellationToken: cancellationToken))
            .Take(20);
        return assets.ToDictionary(a => a.SystemProperties.Id, a => a.Files["en-US"].FileName);
    }
}