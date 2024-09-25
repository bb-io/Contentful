using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Contentful.Core.Models.Management;

namespace Apps.Contentful.DataSourceHandlers;

public class UserDataSourceHandler(InvocationContext invocationContext)
    : ContentfulInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulClient(Creds, null);
        var environments = await client.GetAllUsers(cancellationToken: cancellationToken);

        return environments
            .Where(x => context.SearchString is null ||
                        BuildReadableName(x).Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.SystemProperties.Id, BuildReadableName);
    }
    
    private static string BuildReadableName(User user)
    {
        return $"{user.FirstName} {user.LastName}";
    }
}