using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.DataSourceHandlers.Tags;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Contentful.Models.Requests;

public class GetEntryAsHtmlRequest
{
    [Display("Include referenced entries", Description = "Recursively include content of entries from one or many 'Reference' field types")]
    public bool? GetReferenceContent { get; set; }

    [Display("Ignore localization field for referenced entries", Description = "Include referenecd entries even if the reference field is not marked for localization - only works if 'Include referenced entries' is true")]
    public bool? GetNonLocalizationReferenceContent { get; set; }

    [Display("Include hyperlink entries", Description = "Recursively include content of entries that are referenced as hyperlinks in 'Rich text' fields")]
    public bool? GetHyperlinkContent { get; set; }

    [Display("Include embedded inline entries", Description = "Recursively include content of entries that are referenced as embedded inline entries in 'Rich text' fields")]
    public bool? GetEmbeddedInlineContent { get; set; }

    [Display("Include embedded block entries", Description = "Recursively include content of entries that are referenced as embedded block entries in 'Rich text' fields")]
    public bool? GetEmbeddedBlockContent { get; set; }

    [Display("Include referenced assets", Description = "Include asset content referenced from entry fields")]
    public bool? IncludeReferencedAssets { get; set; } = true;

    [Display("Exclude field IDs globally", Description = "All field IDs in this collection will be omitted from the exported content")]
    public IEnumerable<string>? IgnoredFieldIds { get; set; }

    [Display("Ignore JSON keys", Description = "All matching keys inside JSON object array fields will be preserved but omitted from exported translatable content")]
    public IEnumerable<string>? IgnoredJsonKeys { get; set; }
    
    [Display("Excluded content type IDs", Description = "All content type IDs in this collection will be omitted from the exported content"), DataSource(typeof(ContentModelDataSourceHandler))]
    public IEnumerable<string>? IgnoredContentTypeIds { get; set; }

    [Display("Max depth for referenced entries", Description = "Maximum depth of nested reference entries to include")]
    public int? MaxDepth { get; set; }
    
    [Display("Exclude tags for referenced entries"), DataSource(typeof(ListEntriesTagDataHandler))]
    public IEnumerable<string>? ExcludeTags { get; set; }

    [Display("Conditional field exclusions - Field IDs", 
        Description = "Exclude field IDs of specific content types. Values should correspond to the 'Conditional field exclusions - Content type IDs' input")]
    public IEnumerable<string>? ConditionalIgnoredFieldIds { get; set; }

    [Display("Conditional field exclusions - Content type IDs",
        Description = "Values should correspond to the 'Conditional field exclusions - Field IDs' input")]
    public IEnumerable<string>? ConditionalIgnoredContentTypeIds { get; set; }

    public void Validate()
    {
        var fieldIds = ConditionalIgnoredFieldIds?.ToArray() ?? [];
        var contentTypeIds = ConditionalIgnoredContentTypeIds?.ToArray() ?? [];

        if (fieldIds.Length != contentTypeIds.Length)
        {
            throw new PluginMisconfigurationException(
                "The 'Conditional field exclusions' inputs should have the same length: " +
                $"got {fieldIds.Length} field ID(s) and {contentTypeIds.Length} content type ID(s).");
        }
    }
    
    public Dictionary<string, HashSet<string>> BuildConditionalFieldExclusions()
    {
        var fieldIds = ConditionalIgnoredFieldIds?.ToArray() ?? [];
        var contentTypeIds = ConditionalIgnoredContentTypeIds?.ToArray() ?? [];
        var result = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        for (var i = 0; i < fieldIds.Length; i++)
        {
            if (!result.TryGetValue(contentTypeIds[i], out var excludedFields))
            {
                excludedFields = new HashSet<string>(StringComparer.Ordinal);
                result[contentTypeIds[i]] = excludedFields;
            }

            excludedFields.Add(fieldIds[i]);
        }

        return result;
    }
}
