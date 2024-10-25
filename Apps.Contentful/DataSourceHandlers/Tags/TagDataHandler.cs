using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Requests.Tags;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Contentful.Core.Models.Management;
using RestSharp;

namespace Apps.Contentful.DataSourceHandlers.Tags;

public class TagDataHandler : ContentfulInvocable, IAsyncDataSourceHandler
{
    private string? Environment { get; }
    public TagDataHandler(InvocationContext invocationContext, [ActionParameter] TagRequest tag) : base(invocationContext)
    {
        Environment = tag.Environment;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulRestClient(InvocationContext.AuthenticationCredentialsProviders.ToArray(), Environment);
        var request = new ContentfulRestRequest("tags", Method.Get, InvocationContext.AuthenticationCredentialsProviders.ToArray());
        var response = await client.Paginate<ContentTag>(request);

        return response
            .Where(x => context.SearchString is null ||
                        x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.SystemProperties.Id, x => x.Name);
    }
}