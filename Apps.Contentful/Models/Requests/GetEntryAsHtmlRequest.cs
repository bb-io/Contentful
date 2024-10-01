using Blackbird.Applications.Sdk.Common;

namespace Apps.Contentful.Models.Requests;

public class GetEntryAsHtmlRequest
{
    [Display("Include referenced entries", Description = "Recursively include content of entries from one or many 'Reference' field types")]
    public bool? GetReferenceContent { get; set; }

    [Display("Include hyperlink entries", Description = "Recursively include content of entries that are referenced as hyperlinks in 'Rich text' fields")]
    public bool? GetHyperlinkContent { get; set; }

    [Display("Include embedded inline entries", Description = "Recursively include content of entries that are referenced as embedded inline entries in 'Rich text' fields")]
    public bool? GetEmbeddedInlineContent { get; set; }

    [Display("Include embedded block entries", Description = "Recursively include content of entries that are referenced as embedded block entries in 'Rich text' fields")]
    public bool? GetEmbeddedBlockContent { get; set; }
}