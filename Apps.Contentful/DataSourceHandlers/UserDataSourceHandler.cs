using Apps.Contentful.Api;
using Apps.Contentful.Dtos.Raw;
using Apps.Contentful.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Contentful.DataSourceHandlers;

public class UserDataSourceHandler(InvocationContext invocationContext)
    : ContentfulInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulRestClient(Creds.ToArray(), null);
        var request = new ContentfulRestRequest("/users", Method.Get, Creds);
        var users = await client.Paginate<UserItem>(request);

        return users
            .Where(x => MatchesSearch(x, context.SearchString))
            .ToDictionary(x => x.SystemProperties.Id, BuildReadableName);
    }

    private static bool MatchesSearch(UserItem user, string? searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return true;

        var searchable = string.Join(" ", new[]
        {
            BuildReadableName(user),
            user.Email,
            user.SystemProperties.Id
        }.Where(x => !string.IsNullOrWhiteSpace(x)));

        return searchable.Contains(searchString, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildReadableName(UserItem user)
    {
        var fullName = string.Join(" ", new[] { user.FirstName, user.LastName }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        if (!string.IsNullOrWhiteSpace(fullName) && !string.IsNullOrWhiteSpace(user.Email))
            return $"{fullName} ({user.Email})";

        if (!string.IsNullOrWhiteSpace(fullName))
            return fullName;

        return user.Email ?? user.SystemProperties.Id;
    }
}
