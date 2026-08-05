using Apps.Contentful.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Contentful.Core.Models;
using Newtonsoft.Json.Linq;
using System.Web;

namespace Apps.Contentful.DataSourceHandlers.Base;

public class BaseEntryDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    private string? Environment { get; }

    public BaseEntryDataSourceHandler(InvocationContext invocationContext, string? environment) : base(
        invocationContext)
    {
        Environment = environment;
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders, Environment);

        var queryString = HttpUtility.ParseQueryString(string.Empty);
        if (!string.IsNullOrWhiteSpace(context.SearchString))
            queryString.Add("query", context.SearchString);

        queryString.Add("limit", "30");
        queryString.Add("select", "sys,fields._displayField");

        var entries = (await client.GetEntriesCollection<Entry<dynamic>>($"?{queryString}",
                cancellationToken: cancellationToken))
            .GroupBy(e => e.SystemProperties.ContentType.SystemProperties.Id);
        var entriesDictionary = new Dictionary<string, string>();
        foreach (var entryGroup in entries)
        {
            var contentType = await client.GetContentType(entryGroup.Key, cancellationToken: cancellationToken);

            foreach (var entry in entryGroup)
            {
                var entryId = entry.SystemProperties.Id;
                var entryFields = entry.Fields as JObject;
                var displayFieldName = contentType.DisplayField;
                JToken? displayField = null;

                if (entryFields != null && displayFieldName != null)
                    displayField = entryFields[displayFieldName];

                if (displayField == null && entryFields != null)
                {
                    if (entryFields.Properties().Any())
                    {
                        displayFieldName = entryFields.Properties().First().Name;
                        displayField = entryFields[displayFieldName];
                    }
                }

                var entryDisplayValue = displayField switch
                {
                    JObject localizedValue => localizedValue.Properties().FirstOrDefault()?.Value.ToString() ?? entryId,
                    null => entryId,
                    _ => displayField.ToString()
                };
                entryDisplayValue = contentType.Name + ": " + entryDisplayValue;
                entriesDictionary[entryId] = entryDisplayValue;

                if (entriesDictionary.Count >= 30)
                    return entriesDictionary;
            }
        }

        return entriesDictionary;
    }
}
