using Apps.Contentful.Api;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Contentful.DataSourceHandlers;

public class JsonFieldDataHandler(
    InvocationContext invocationContext,
    [ActionParameter] EntryLocaleIdentifier entryIdentifier)
    : BaseInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(
        DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(entryIdentifier.EntryId))
        {
            throw new Exception("Please provide Entry ID first");
        }

        var client = new ContentfulClient(
            InvocationContext.AuthenticationCredentialsProviders,
            entryIdentifier.Environment);
        var entry = await client.ExecuteWithErrorHandling(async () =>
            await client.GetEntry(entryIdentifier.EntryId, cancellationToken: cancellationToken));
        var contentTypeId = entry.SystemProperties.ContentType.SystemProperties.Id;
        var contentType = await client.ExecuteWithErrorHandling(async () =>
            await client.GetContentType(contentTypeId, cancellationToken: cancellationToken));

        return contentType.Fields
            .Where(field => string.Equals(field.Type, "Object", StringComparison.OrdinalIgnoreCase))
            .Where(field => context.SearchString == null ||
                            field.Id.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase) ||
                            field.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(field => new DataSourceItem(field.Id, field.Name));
    }
}
