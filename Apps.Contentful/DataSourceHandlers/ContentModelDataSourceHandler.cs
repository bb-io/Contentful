﻿using Apps.Contentful.Api;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class ContentModelDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private string? Environment { get; }

    public ContentModelDataSourceHandler(InvocationContext invocationContext,
        [ActionParameter] ContentModelIdentifier identifier) : base(invocationContext)
    {
        Environment = identifier.Environment;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders, Environment);
        var contentModels = (await client.GetContentTypes(cancellationToken: cancellationToken))
            .Where(m => context.SearchString == null ||
                        m.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase));
        return contentModels.ToDictionary(m => m.SystemProperties.Id, m => m.Name);
    }
}