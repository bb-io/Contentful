using Apps.Contentful.Api;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class LocaleDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private string? Environment { get; }

    public LocaleDataSourceHandler(InvocationContext invocationContext, [ActionParameter] LocaleIdentifier identifier) :
        base(invocationContext)
    {
        Environment = identifier.Environment;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders, Environment);
        var locales = (await client.GetLocalesCollection(cancellationToken: cancellationToken))
            .Where(l => context.SearchString == null ||
                        l.Code.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));
        return locales.ToDictionary(l => l.Code, l => l.Name);
    }
}