using Apps.Contentful.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Contentful.Core.Models;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.DataSourceHandlers;

public class EntryDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public EntryDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new ContentfulClient(InvocationContext.AuthenticationCredentialsProviders);
        
        var entries = (await client.GetEntriesCollection<Entry<dynamic>>($"?query={context.SearchString}", 
                cancellationToken: cancellationToken)).Take(20)
            .GroupBy(e => e.SystemProperties.ContentType.SystemProperties.Id);
        var entriesDictionary = new Dictionary<string, string>();
        foreach (var entryGroup in entries)
        {
            var contentType = await client.GetContentType(entryGroup.Key, cancellationToken: cancellationToken);
            
            foreach (var entry in entryGroup)
            {
                var entryId = entry.SystemProperties.Id;
                var entryFields = (JObject)entry.Fields;
                var displayFieldName = contentType.DisplayField;
                JToken? displayField = null;
            
                if (displayFieldName != null) 
                    displayField = entryFields[displayFieldName];
            
                if (displayField == null)
                {
                    if (entryFields.Properties().Any())
                    {
                        displayFieldName = entryFields.Properties().First().Name;
                        displayField = entryFields[displayFieldName];
                    }
                }
            
                var entryDisplayValue = displayField == null ? entryId : displayField["en-US"].ToString();
                entryDisplayValue = contentType.Name + ": " + entryDisplayValue;
                entriesDictionary[entryId] = entryDisplayValue;
            }
        }

        return entriesDictionary;
    }
}