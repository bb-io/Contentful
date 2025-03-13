using Blackbird.Applications.Sdk.Common;
using Contentful.Core.Models;
using Newtonsoft.Json.Linq;

namespace Apps.Contentful.Models.Entities;

public class EntryWithTitleEntity : EntryEntity
{
    [Display("Entry title")]
    public string Title { get; set; }

    public EntryWithTitleEntity(Entry<object> entry, string? locale = null) : base(entry)
    {
        Title = GetEntryTitle(entry, locale);
    }

    private static string GetEntryTitle(Entry<object> entry, string? locale)
    {
        var displayField = entry.SystemProperties.ContentType.DisplayField;
        if (string.IsNullOrEmpty(displayField))
        {
            return string.Empty;
        }

        var fields = JObject.FromObject(entry.Fields);
        
        if(!string.IsNullOrEmpty(locale))
        {
            return fields[displayField]?[locale]?.ToString() ?? string.Empty;
        }
        
        if (fields[displayField] is JObject jsonObject && jsonObject.HasValues)
        {
            var firstProperty = jsonObject.Properties().FirstOrDefault();
            return firstProperty?.Value.ToString() ?? string.Empty;
        }
        
        return string.Empty;
    }
}
