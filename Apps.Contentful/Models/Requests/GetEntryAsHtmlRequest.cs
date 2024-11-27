using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

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

    [Display("Exclude field IDs", Description = "All field IDs in this collection will be omitted from the exported content")]
    public IEnumerable<string>? IgnoredFieldIds { get; set; }
    
    [Display("Excluded content type IDs", Description = "All content type IDs in this collection will be omitted from the exported content"), DataSource(typeof(ContentModelDataSourceHandler))]
    public IEnumerable<string>? IgnoredContentTypeIds { get; set; }
}