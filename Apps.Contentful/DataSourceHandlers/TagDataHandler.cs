using Apps.Contentful.Api;
using Apps.Contentful.Invocables;
using Apps.Contentful.Models.Requests.Tags;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

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
        var client = new ContentfulClient(Creds, Environment);
        var response = await client.GetContentTagsCollection(cancellationToken: cancellationToken);

        return response
            .Where(x => context.SearchString is null ||
                        x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.SystemProperties.Id, x => x.Name);
    }
}